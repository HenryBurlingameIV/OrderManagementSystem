using AutoFixture;
using Microsoft.AspNetCore.Http;
using OrderManagementSystem.Shared.Enums;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.OrderFixture
{
    public class OrderFixtureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<OrderItem>(composer => composer
                .FromFactory(() =>
                {
                    var item = new OrderItem()
                    {
                        ProductId = Guid.NewGuid(),
                        Price = fixture.Create<decimal>() % 1000 + 1,
                        Quantity = fixture.Create<int>() % 1000 + 1,
                    };
                    return item;
                })
                .OmitAutoProperties()


            );

            var createdAt = DateTime.UtcNow;
            fixture.Customize<Order>(composer => composer
                .FromFactory(() =>
                {
                    var order = new Order()
                    {
                        Id = Guid.NewGuid(),
                        CreatedAtUtc = createdAt,
                        UpdatedAtUtc = createdAt,
                        Items = fixture
                            .CreateMany<OrderItem>(fixture.Create<int>() % 10 + 1)
                            .ToList(),
                        Status = OrderStatus.New,
                        Email = "test@gmail.com"
                    };

                    order.TotalPrice = order.Items.Sum(oi => oi.Price * oi.Quantity);
                    return order;
                })
                .OmitAutoProperties());
        }
    }
}
