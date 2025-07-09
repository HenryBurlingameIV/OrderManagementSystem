using FluentValidation;
using Moq;
using OrderManagementSystem.Shared.Contracts;
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

namespace OrderService.Tests.UnitTests
{
    public class CreateOrderCommandHandlerTests
    {
        private CreateOrderCommandHandler _handler;
        private IValidator<CreateOrderCommand> _validator;
        private Mock<IRepository<Order>> _mockRepository;
        private Mock<IKafkaProducer<OrderEvent>> _mockKafkaProducer;
        private Mock<ICatalogServiceApi> _mockCatalogServiceApi;
        public CreateOrderCommandHandlerTests() 
        {
            _validator = new CreateOrderCommandValidator();
            _mockRepository = new Mock<IRepository<Order>>();
            _mockKafkaProducer = new Mock<IKafkaProducer<OrderEvent>>();
            _mockCatalogServiceApi = new Mock<ICatalogServiceApi>();
            _handler = new CreateOrderCommandHandler(
                _mockRepository.Object, 
                _mockCatalogServiceApi.Object, 
                _validator, 
                _mockKafkaProducer.Object);
        }

        private List<OrderItemRequest> GenerateOrderItemRequests(int count)
        {
            var random = new Random();
            return Enumerable
                .Range(0, count)
                .Select(i =>
                    new OrderItemRequest(Guid.NewGuid(), random.Next(1, 10)))   
                .ToList();
        }

        private static bool IsValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }

        [Fact]
        public async Task Should_CreateOrder_WhenRequestIsValid()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = GenerateOrderItemRequests(5)
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


            _mockKafkaProducer
                .Setup(producer => producer.ProduceAsync(
                    It.Is<string>(id => IsValidGuid(id)),
                    It.Is<OrderEvent>(e => e.Status == "New" && e.TotalPrice == expectedTotalPrice),
                    It.IsAny<CancellationToken>()))
                .Verifiable();

            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _mockCatalogServiceApi.VerifyAll();
            _mockKafkaProducer.VerifyAll();
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenOrderItemsIsEmpty()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = new List<OrderItemRequest>()
            };

            //Act && Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(command, CancellationToken.None));
            Assert.Contains("'Order Items' must not be empty", exception.Message);
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenOrderItemsIsNull()
        {
            //Arrange
            var command = new CreateOrderCommand()
            {
                OrderItems = null
            };

            //Act && Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(command, CancellationToken.None));
            Assert.Contains("'Order Items' must not be empty", exception.Message);
        }
    }
}
