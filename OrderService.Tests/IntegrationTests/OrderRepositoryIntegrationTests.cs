using Microsoft.AspNetCore.Mvc.ApplicationParts;
using OrderService.Domain.Entities;
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

        public OrderRepositoryIntegrationTests(OrderRepositoryFixture fixture) 
        {
            _fixture = fixture;
        }
        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDataBase();
        }

        private static decimal GenerateRandomPrice(decimal minValue = 0m, decimal maxValue = 1000m, int decimalPlaces = 2)
        {
            var random = new Random();
            decimal range = maxValue - minValue;
            decimal randomValue = (decimal)random.NextDouble() * range + minValue;
            return Math.Round(randomValue, decimalPlaces);
        }

        private static int GenerateRandomQuantity(int maxValue = 100)
        {
            var random = new Random();
            return random.Next(1, maxValue + 1);
        }


        private Order CreateSampleOrder(int itemsCount)
        {
            var createdAt = DateTime.UtcNow;
            var orderItems = GenerateOrderItems(itemsCount);
            return new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.New,
                Items = orderItems,
                TotalPrice = orderItems.Sum(i => i.Quantity * i.Price),
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = createdAt
            };
        }

        private List<OrderItem> GenerateOrderItems(int count)
        {

            return Enumerable
                .Range(0, count)
                .Select(i =>
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        Price = GenerateRandomPrice(),
                        Quantity = GenerateRandomQuantity()
                    })
                .ToList();                   
        }

        private List<Order> GenerateSampleOrders(int ordersCount, int maxItemsPerOrder = 10)
        {
            return Enumerable
                .Range(0, ordersCount)
                .Select (i => CreateSampleOrder(GenerateRandomQuantity(maxItemsPerOrder)))
                .ToList();
        }

        [Fact]
        public async Task Should_ReturnOrderIdAndSaveToDatabase_WhenCreatingNewOrder()
        {
            //Arrange
            var order = CreateSampleOrder(5);
            var expectedId = order.Id;

            //Act
            var actualId = await _fixture.OrderRepository.CreateAsync(order, CancellationToken.None);
            
            //Assert
            Assert.Equal(expectedId, actualId);
            var savedOrder = await _fixture.DbContext.Orders.FindAsync(order.Id);
            Assert.NotNull(savedOrder);
            Assert.Equal(expectedId, savedOrder.Id);
            Assert.Equal(order.Status, savedOrder.Status); 
            Assert.Equal(order.TotalPrice, savedOrder.TotalPrice);
            Assert.Equal(order.Items.Count, savedOrder.Items.Count);
        }

        [Fact]
        public async Task Should_ReturnOrder_WhenOrderExists()
        {
            //Arrange
            var orders = GenerateSampleOrders(3).ToArray();
            await _fixture.DbContext.Orders.AddRangeAsync(orders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);

            //Act
            var actual = await _fixture.OrderRepository.GetByIdAsync(orders[0].Id, CancellationToken.None);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(orders[0].Id, actual.Id);
            Assert.Equal(orders[0].Items.Count, actual.Items.Count);
            Assert.Equal(orders[0].Status, actual.Status);
            Assert.Equal(orders[0].Items[0].ProductId, actual.Items[0].ProductId);
        }





    }
}
