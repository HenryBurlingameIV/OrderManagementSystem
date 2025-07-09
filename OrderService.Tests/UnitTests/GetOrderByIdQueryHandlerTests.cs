using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.DTO;
using OrderService.Application.Queries.OrderQuery;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.UnitTests
{
    public class GetOrderByIdQueryHandlerTests
    {
        private Mock<IRepository<Order>> _mockOrderRepository;
        private GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IRepository<Order>>();
            _handler = new GetOrderByIdQueryHandler(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task Should_ReturnOrderViewModel_WhenOrderIsExists()
        {
            //Arrange
            var orders = OrderFactory.GenerateSampleOrders(3);

            var request = new GetOrderByIdQuery(orders[0].Id);
            _mockOrderRepository
                .Setup(repo => repo.GetByIdAsync(
                    It.Is<Guid>(id => id == orders[0].Id),
                    CancellationToken.None))
                .ReturnsAsync((Guid id, CancellationToken token) => orders.FirstOrDefault(o => o.Id == id));

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OrderViewModel>(result);
            Assert.Equal(orders[0].Id, result.Id);
            Assert.Equal(orders[0].Status.ToString(), result.Status);
            Assert.Equal(orders[0].Items.Count(), result.Items.Count());
            Assert.Equal(orders[0].TotalPrice, result.TotalPrice);
            Assert.Equal(orders[0].CreatedAtUtc, result.CreatedAtUtc);
            Assert.Equal(orders[0].UpdatedAtUtc, result.UpdatedAtUtc);
            _mockOrderRepository.VerifyAll();
        }




    }
}
