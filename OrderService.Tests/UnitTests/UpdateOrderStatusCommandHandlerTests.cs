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

        [Fact]
        public async Task Should_UpdateOrderStatus_WhenNewStatusIsValid()
        {
            //Arrange
            var order = OrderFactory.CreateSampleOrder(3);

            var newOrderStatus = OrderStatus.Processing;

            var command = new UpdateOrderStatusCommand(order.Id, newOrderStatus);

            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    It.Is<Guid>(id => id == order.Id),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockOrderRepository
                .Setup(repo => repo.UpdateAsync(
                    It.Is<Order>(o => o.Id == order.Id && o.Status == newOrderStatus),
                    It.IsAny<CancellationToken>()))
                .Verifiable();



            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _mockOrderRepository.VerifyAll();
            Assert.Equal(newOrderStatus, order.Status);
        }        
    }
}
