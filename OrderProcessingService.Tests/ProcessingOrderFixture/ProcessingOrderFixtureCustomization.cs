using AutoFixture;
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
                        ProductId = Guid.NewGuid(),
                        Quantity = (int)fixture.Create<uint>() % 1000 + 1,
                        Status = ItemAssemblyStatus.Pending,
                    };
                })
                .OmitAutoProperties());

            var createdAt = DateTime.UtcNow;
            fixture.Customize<ProcessingOrder>(composer => composer
                .With(po => po.Id, () => Guid.NewGuid())
                .With(po => po.OrderId, () => Guid.NewGuid())
                .With(po => po.Stage, () => Stage.Assembly)
                .With(po => po.Status, () => ProcessingStatus.New)
                .With(po => po.CreatedAt, createdAt)
                .With(po => po.UpdatedAt, createdAt)
                .With(po => po.TrackingNumber, () => null)
                .With(po => po.Items, () =>
                {
                    return fixture
                        .CreateMany<OrderItem>((int)fixture.Create<uint>() % 99 + 1)
                        .ToList();
                })
                .OmitAutoProperties()
                );                                
        }
    }
}
