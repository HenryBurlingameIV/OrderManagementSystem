using FluentValidation;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
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
    public class UpdateOrderStatusCommandHandlerTests
    {
        private IValidator<OrderStatusValidationModel> _validator;
        private Mock<IRepository<Order>> _mockOrderRepository;
        private UpdateOrderStatusCommandHandler _handler;


        public UpdateOrderStatusCommandHandlerTests()
        {
            _validator = new OrderStatusTransitionValidator();
            _mockOrderRepository = new Mock<IRepository<Order>>();
            _handler = new UpdateOrderStatusCommandHandler(_mockOrderRepository.Object, _validator);
        }

        [Theory]
        [InlineData(OrderStatus.New, OrderStatus.Processing)]
        [InlineData(OrderStatus.Processing, OrderStatus.Ready)]
        [InlineData(OrderStatus.Ready, OrderStatus.Delivering)]
        [InlineData(OrderStatus.Delivering, OrderStatus.Delivered)]
        [InlineData(OrderStatus.New, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Ready, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Delivering, OrderStatus.Cancelled)]
        public async Task Should_UpdateOrderStatus_WhenNewStatusIsValid(OrderStatus from, OrderStatus to)
        {
            //Arrange
            var order = OrderFactory.CreateSampleOrder(3);
            order.Status = from;
            var initialUpdatedAt = order.UpdatedAtUtc;
            var initialTotalPrice = order.TotalPrice;
            var command = new UpdateOrderStatusCommand(order.Id, to);

            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    It.Is<Guid>(id => id == order.Id),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockOrderRepository
                .Setup(repo => repo.UpdateAsync(
                    It.Is<Order>(o => o.Id == order.Id && o.Status == to),
                    It.IsAny<CancellationToken>()))
                .Verifiable();


            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _mockOrderRepository.VerifyAll();
            Assert.Equal(to, order.Status);
            Assert.True(order.UpdatedAtUtc > initialUpdatedAt);
            Assert.Equal(initialTotalPrice, order.TotalPrice);
        }


        [Theory]
        [InlineData(OrderStatus.New, OrderStatus.New)]
        [InlineData(OrderStatus.Processing, OrderStatus.Processing)]
        [InlineData(OrderStatus.Ready, OrderStatus.Ready)]
        [InlineData(OrderStatus.Delivering, OrderStatus.Delivering)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.New)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Ready)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Delivering)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Delivered, OrderStatus.New)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Processing)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Ready)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Delivering)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
        public async Task Should_ThrowValidationException_WhenNewStatusIsInvalid(OrderStatus from, OrderStatus to)
        {
            //Arrange
            var order = OrderFactory.CreateSampleOrder(3);
            order.Status = from;
            var initialUpdatedAt = order.UpdatedAtUtc;
            var initialTotalPrice = order.TotalPrice;
            var command = new UpdateOrderStatusCommand(order.Id, to);

            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    It.Is<Guid>(id => id == order.Id),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockOrderRepository
                .Setup(repo => repo.UpdateAsync(
                    It.Is<Order>(o => o.Id == order.Id && o.Status == to),
                    It.IsAny<CancellationToken>()))
                .Verifiable();

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("Cannot change order status from", exception.Message);
        }

    }
}
