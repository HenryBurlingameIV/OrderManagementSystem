using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests
{
    public class OrderFactory
    {
        private static decimal GenerateRandomPrice(decimal minValue = 10m, decimal maxValue = 1000m, int decimalPlaces = 2)
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


        public static Order CreateSampleOrder(int itemsCount)
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

        public static List<OrderItem> GenerateOrderItems(int count)
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

        public static List<Order> GenerateSampleOrders(int ordersCount, int maxItemsPerOrder = 10)
        {
            return Enumerable
                .Range(0, ordersCount)
                .Select(i => CreateSampleOrder(GenerateRandomQuantity(maxItemsPerOrder)))
                .ToList();
        }
    }
}
