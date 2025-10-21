using AutoFixture;
using Castle.Core.Logging;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Application.Services;
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
        private IValidator<CreateOrderRequest> _validator;
        private Mock<IEFRepository<Order, Guid>> _mockRepository;
        private Mock<IKafkaProducer<OrderEvent>> _mockOrderProducer;
        private Mock<ICatalogServiceApi> _mockCatalogServiceApi;
        private Mock<ILogger<CreateOrderCommandHandler>> _mockHandlerLogger;
        private Mock<ILogger<OrderFactory>> _mockFactoryLogger;
        private Mock<IKafkaProducer<OrderStatusEvent>> _mockOrderStatusProducer;
        private OrderFactory _orderFactory;

        public CreateOrderCommandHandlerTests() 
        {
            _autoFixture = new Fixture();
            _validator = new CreateOrderRequestValidator();
            _mockRepository = new Mock<IEFRepository<Order, Guid>>();
            _mockOrderProducer = new Mock<IKafkaProducer<OrderEvent>>();
            _mockOrderStatusProducer  = new Mock<IKafkaProducer<OrderStatusEvent>>();
            _mockCatalogServiceApi = new Mock<ICatalogServiceApi>();
            _mockHandlerLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
            _mockFactoryLogger = new Mock<ILogger<OrderFactory>>();

            _autoFixture.Customize<OrderItemRequest>(composer => composer
                .With(x => x.Quantity, () => Random.Shared.Next(1, 1001))
            );

            _orderFactory = new OrderFactory(_mockCatalogServiceApi.Object, _mockFactoryLogger.Object);
            _handler = new CreateOrderCommandHandler(
                _mockRepository.Object,
                _orderFactory, 
                _validator, 
                _mockOrderProducer.Object,
                _mockOrderStatusProducer.Object,
                _mockHandlerLogger.Object);
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

        private CreateOrderCommand BuildOrderCommand(
            int itemsCount = 3, string email = "test@gmail.com")
        {
            return new CreateOrderCommand(
               new CreateOrderRequest(
                   _autoFixture.CreateMany<OrderItemRequest>(itemsCount).ToList(),
                   email
                   ));
        }

        [Fact]
        public async Task Should_CreateOrder_WhenRequestIsValid()
        {
            //Arrange
            var command = BuildOrderCommand();
            var expectedTotalPrice = command.Request.Items.Sum(i => i.Quantity * 100m);
            _mockCatalogServiceApi
                .Setup(api => api.ReserveProductAsync(
                    It.Is<Guid>(id => command.Request.Items.Any(item => item.Id == id)),
                    It.Is<int>(q => command.Request.Items.Any(item => item.Quantity == q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, int quantity, CancellationToken cancellationToken) =>
                {
                    return new ProductDto(id, 100m, quantity);
                });

            _mockRepository
                .Setup(repo => repo.InsertAsync(
                    It.Is<Order>(o => 
                        o.Items.Count == command.Request.Items.Count &&
                        o.Status == OrderStatus.New &&
                        o.TotalPrice == expectedTotalPrice),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order order, CancellationToken cancellationToken) =>
                    order);


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
            var command = BuildOrderCommand();
            command.Request.Items.Add(new OrderItemRequest(Guid.NewGuid(), 1001));
   
            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("1000", exception.Message);
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenOrderItemsIsEmpty()
        {
            //Arrange
            var command = BuildOrderCommand(0);

            //Act && Assert
            await AssertValidationException(command, "'Items' must not be empty", 1);
        }


        [Fact]
        public async Task Should_ThrowValidationException_WhenEmailFormatIsInvalid()
        {
            //Arrange
            var command = BuildOrderCommand(email: "not_email");


            //Act && Assert
            await AssertValidationException(command, "Invalid email format.", 1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Should_ThrowValidationException_WhenItemQuantityIsInvalid(int invalidQuantity)
        {
            //Arrange
            var command = BuildOrderCommand(0);
            command.Request.Items.Add(new OrderItemRequest(Guid.NewGuid(), invalidQuantity));

            //Act & Assert
            await AssertValidationException(command, "'Quantity' must be greater than '0'", 1);
        }
    }
}
