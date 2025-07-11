using AutoFixture;
using Microsoft.AspNetCore.Http;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests
{
    public class OrderFixtureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<OrderItem>(composer => composer
                .With(oi => oi.Price, fixture.Create<decimal>() % 10000 + 1)
                .With(pi => pi.Quantity, fixture.Create<int>() % 100 + 1)
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
                        Status = OrderStatus.New
                    };

                    order.TotalPrice = order.Items.Sum(oi => oi.Price * oi.Quantity);
                    return order;
                }));
        }
    }
}
