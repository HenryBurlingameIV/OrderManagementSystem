using AutoFixture;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.ProcessingOrderFixture
{
    public class ProcessingOrderFixtureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<OrderItem>(composer => composer
                .FromFactory(() =>
                {
                    return new OrderItem()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        ProcessingOrderId = Guid.NewGuid(),
                        Quantity = Random.Shared.Next(1, 101),
                        Status = ItemAssemblyStatus.Pending,
                    };
                })
                .OmitAutoProperties());

            var createdAt = DateTime.UtcNow;
            fixture.Customize<ProcessingOrder>(composer => composer
                .FromFactory(() =>
                {
                    var processingOrderId = Guid.NewGuid();
                    return new ProcessingOrder
                    {
                        Id = processingOrderId,
                        OrderId = Guid.NewGuid(),
                        Stage = Stage.Assembly,
                        Status = ProcessingStatus.New,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt,
                        TrackingNumber = null,
                        Items = fixture.Build<OrderItem>()
                            .With(x => x.ProcessingOrderId, processingOrderId)
                            .With(x => x.Status, ItemAssemblyStatus.Pending)
                            .CreateMany(Random.Shared.Next(1, 6))
                            .ToList()
                    };
                })
                .OmitAutoProperties());

            fixture.Customize<OrderItemDto>(composer => composer
                .With(i => i.ProductId, () => Guid.NewGuid())
                .With(i => i.Quantity, () => Random.Shared.Next(1, 101))
                .OmitAutoProperties());

            fixture.Customize<OrderDto>(composer => composer
                .With(o => o.Items, () => fixture.CreateMany<OrderItemDto>(Random.Shared.Next(1, 6)).ToList())
                .With(o => o.Id, () => Guid.NewGuid())
                .With(o => o.CreatedAt, createdAt)
                .With(o => o.UpdatedAt, createdAt)
                .OmitAutoProperties());
        }
    }
}
