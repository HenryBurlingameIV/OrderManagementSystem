using Microsoft.AspNetCore.Mvc.ApplicationParts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Domain.Entities;
using OrderService.Tests.OrderFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.IntegrationTests
{
    public class OrderRepositoryIntegrationTests : IAsyncLifetime, IClassFixture<OrderRepositoryFixture>
    {
        private readonly OrderRepositoryFixture _fixture;

        public OrderRepositoryIntegrationTests(OrderRepositoryFixture orderRepositoryFixture) 
        {
            _fixture = orderRepositoryFixture;
        }
        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDataBase();
        }


        [Theory, AutoOrderData]
        public async Task Should_ReturnOrderIdAndSaveToDatabase_WhenCreatingNewOrder(Order order)
        {
            //Arrange
            var expectedId = order.Id;

            //Act
            var resultId = await _fixture.OrderRepository.CreateAsync(order, CancellationToken.None);
            
            //Assert
            Assert.Equal(expectedId, resultId);
            var savedOrder = await _fixture.DbContext.Orders.FindAsync(order.Id);
            Assert.NotNull(savedOrder);
            Assert.Equal(expectedId, savedOrder.Id);
            Assert.Equal(order.Status, savedOrder.Status); 
            Assert.Equal(order.TotalPrice, savedOrder.TotalPrice);
            Assert.Equal(order.Items.Count, savedOrder.Items.Count);
        }

        [Theory, AutoOrderData]
        public async Task Should_ReturnOrder_WhenOrderExists(Order[] orders)
        {
            //Arrange
            await _fixture.DbContext.Orders.AddRangeAsync(orders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);

            //Act
            var result = await _fixture.OrderRepository.GetByIdAsync(orders[0].Id, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(orders[0].Id, result.Id);
            Assert.Equal(orders[0].Items.Count, result.Items.Count);
            Assert.Equal(orders[0].Status, result.Status);
            Assert.Equal(orders[0].Items[0].ProductId, result.Items[0].ProductId);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenOrderNotFound()
        {
            //Arrange
            var orderId = Guid.NewGuid();

            //Act 
            var result = await _fixture.OrderRepository.GetByIdAsync(orderId, CancellationToken.None);

            //Assert
            Assert.Null(result);          
        }

        [Theory, AutoOrderData]
        public async Task Should_UpdateOrderStatusAndTimestamps_WhenOrderExists(Order order)
        {
            //Arrange
            await _fixture.DbContext.AddAsync(order, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync();
            var originalUpdatedAt = order.UpdatedAtUtc;
            var newStatus = OrderStatus.Processing;

            //Act
            order.Status = newStatus;
            order.UpdatedAtUtc = DateTime.UtcNow;
            var resultId = await _fixture.OrderRepository.UpdateAsync(order, CancellationToken.None);

            //Assert
            var updatedOrder = await _fixture.DbContext.Orders.FindAsync(resultId);
            Assert.NotNull(updatedOrder);
            Assert.Equal(order.Id, resultId);
            Assert.Equal(newStatus, updatedOrder.Status);
            Assert.True(updatedOrder.UpdatedAtUtc > originalUpdatedAt);
            Assert.Equal(order.TotalPrice, updatedOrder.TotalPrice);
        }





    }
}
