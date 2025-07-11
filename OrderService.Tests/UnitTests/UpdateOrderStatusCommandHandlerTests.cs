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
        [InlineAutoOrderData(OrderStatus.New, OrderStatus.Processing)]
        [InlineAutoOrderData(OrderStatus.Processing, OrderStatus.Ready)]
        [InlineAutoOrderData(OrderStatus.Ready, OrderStatus.Delivering)]
        [InlineAutoOrderData(OrderStatus.Delivering, OrderStatus.Delivered)]
        [InlineAutoOrderData(OrderStatus.New, OrderStatus.Cancelled)]
        [InlineAutoOrderData(OrderStatus.Processing, OrderStatus.Cancelled)]
        [InlineAutoOrderData(OrderStatus.Ready, OrderStatus.Cancelled)]
        [InlineAutoOrderData(OrderStatus.Delivering, OrderStatus.Cancelled)]
        public async Task Should_UpdateOrderStatus_WhenNewStatusIsValid(OrderStatus from, OrderStatus to, Order order)
        {
            //Arrange
            //var order = OrderFactory.CreateSampleOrder(3);
            order.Status = from;
            var initialUpdatedAt = order.UpdatedAtUtc;
            var initialTotalPrice = order.TotalPrice;
            var command = new UpdateOrderStatusCommand(order.Id, to);

            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    order.Id,
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
        [InlineAutoOrderData(OrderStatus.New, OrderStatus.New)]
        [InlineAutoOrderData(OrderStatus.Processing, OrderStatus.Processing)]
        [InlineAutoOrderData(OrderStatus.Ready, OrderStatus.Ready)]
        [InlineAutoOrderData(OrderStatus.Delivering, OrderStatus.Delivering)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.Delivered)]
        [InlineAutoOrderData(OrderStatus.Cancelled, OrderStatus.Cancelled)]
        [InlineAutoOrderData(OrderStatus.Cancelled, OrderStatus.New)]
        [InlineAutoOrderData(OrderStatus.Cancelled, OrderStatus.Ready)]
        [InlineAutoOrderData(OrderStatus.Cancelled, OrderStatus.Delivering)]
        [InlineAutoOrderData(OrderStatus.Cancelled, OrderStatus.Delivered)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.New)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.Processing)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.Ready)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.Delivering)]
        [InlineAutoOrderData(OrderStatus.Delivered, OrderStatus.Cancelled)]
        public async Task Should_ThrowValidationException_WhenNewStatusIsInvalid(OrderStatus from, OrderStatus to, Order order)
        {
            //Arrange
            order.Status = from;
            var initialUpdatedAt = order.UpdatedAtUtc;
            var initialTotalPrice = order.TotalPrice;
            var command = new UpdateOrderStatusCommand(order.Id, to);

            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);


            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("Cannot change order status from", exception.Message);
            _mockOrderRepository.VerifyAll();
        }

    }
}
