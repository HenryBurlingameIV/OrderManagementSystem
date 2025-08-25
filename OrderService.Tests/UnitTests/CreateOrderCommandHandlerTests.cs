using AutoFixture;
using FluentValidation;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OrderService.Tests.UnitTests
{
    public class CreateOrderCommandHandlerTests
    {
        private CreateOrderCommandHandler _handler;
        private Fixture _autoFixture;
        private IValidator<CreateOrderCommand> _validator;
        private Mock<IRepository<Order>> _mockRepository;
        private Mock<IKafkaProducer<OrderEvent>> _mockOrderProducer;
        private Mock<ICatalogServiceApi> _mockCatalogServiceApi;
        private Mock<IKafkaProducer<OrderStatusEvent>> _mockOrderStatusProducer;

        public CreateOrderCommandHandlerTests() 
        {
            _autoFixture = new Fixture();
            _autoFixture.Customize<OrderItemRequest>(composer => composer
                .With(x => x.Quantity, () => new Random().Next(1, 1001))
            );
            _validator = new CreateOrderCommandValidator();
            _mockRepository = new Mock<IRepository<Order>>();
            _mockOrderProducer = new Mock<IKafkaProducer<OrderEvent>>();
            _mockOrderStatusProducer  = new Mock<IKafkaProducer<OrderStatusEvent>>();
            _mockCatalogServiceApi = new Mock<ICatalogServiceApi>();
            _handler = new CreateOrderCommandHandler(
                _mockRepository.Object, 
                _mockCatalogServiceApi.Object, 
                _validator, 
                _mockOrderProducer.Object,
                _mockOrderStatusProducer.Object);
        }


        private static bool IsValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }

        private async Task AssertValidationException(CreateOrderCommand command, string expectedMessage, int expectedErrorCount)
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains(expectedMessage, exception.Message);
            Assert.Equal(expectedErrorCount, exception.Errors.Count());
        }

        [Fact]
        public async Task Should_CreateOrder_WhenRequestIsValid()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = _autoFixture.CreateMany<OrderItemRequest>(5).ToList(),
                Email = "test@gmail.com"
            };

            var expectedTotalPrice = command.OrderItems.Sum(i => i.Quantity * 100m);
            _mockCatalogServiceApi
                .Setup(api => api.ReserveProductAsync(
                    It.Is<Guid>(id => command.OrderItems.Any(item => item.Id == id)),
                    It.Is<int>(q => command.OrderItems.Any(item => item.Quantity == q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, int quantity, CancellationToken cancellationToken) =>
                {
                    return new ProductDto(id, 100m, quantity);
                });

            _mockRepository
                .Setup(repo => repo.CreateAsync(
                    It.Is<Order>(o => 
                        o.Items.Count == command.OrderItems.Count &&
                        o.Status == OrderStatus.New &&
                        o.TotalPrice == expectedTotalPrice),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order order, CancellationToken cancellationToken) =>
                    order.Id);


            _mockOrderProducer
                .Setup(producer => producer.ProduceAsync(
                    It.Is<string>(id => IsValidGuid(id)),
                    It.Is<OrderEvent>(e => e.Status == "New" && e.TotalPrice == expectedTotalPrice),
                    It.IsAny<CancellationToken>()))
                .Verifiable();

            _mockOrderStatusProducer
                .Setup(producer => producer.ProduceAsync(
                    It.Is<string>(id => IsValidGuid(id)),
                    It.Is<OrderStatusEvent>(e => e.OrderStatus == (int)OrderStatus.New),
                    It.IsAny<CancellationToken>()))
                .Verifiable();


            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _mockCatalogServiceApi.VerifyAll();
            _mockOrderProducer.VerifyAll();
            _mockRepository.VerifyAll();
            _mockOrderStatusProducer.VerifyAll();
        }


        [Fact]
        public async Task Should_ThrowValidationException_WhenAnyItemQuantityExceedsMaximumAllowed()
        {
            //Arange
            var command = new CreateOrderCommand()
            {
                OrderItems = _autoFixture.CreateMany<OrderItemRequest>(3).ToList(),
                Email = "test@gmail.com"
            };
            command.OrderItems.Add(new OrderItemRequest(Guid.NewGuid(), 1001));

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("1000", exception.Message);
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenOrderItemsIsEmpty()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = new List<OrderItemRequest>(),
                Email = "test@gmail.com"
            };

            //Act && Assert
            await AssertValidationException(command, "'Order Items' must not be empty", 1);
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenOrderItemsIsNull()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = null,
                Email = "test@gmail.com"
            };

            //Act && Assert
            await AssertValidationException(command, "'Order Items' must not be empty", 1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Should_ThrowValidationException_WhenItemQuantityIsInvalid(int invalidQuantity)
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = new()
                {
                    new OrderItemRequest(Guid.NewGuid(), invalidQuantity),
                },
                Email = "test@gmail.com"
            };

            //Act & Assert
            await AssertValidationException(command, "'Quantity' must be greater than '0'", 1);
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenEmailFormatIsInvalid()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = _autoFixture.CreateMany<OrderItemRequest>(3).ToList(),
                Email = "not_email"
            };

            //Act && Assert
            await AssertValidationException(command, "Invalid email format.", 1);
        }
    }
}
