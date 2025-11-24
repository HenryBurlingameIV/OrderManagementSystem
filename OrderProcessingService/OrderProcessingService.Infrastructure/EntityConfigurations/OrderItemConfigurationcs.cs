using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.EntityConfigurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(oi => oi.Id); 

            builder.Property(oi => oi.ProcessingOrderId).IsRequired();
            builder.Property(oi => oi.ProductId).IsRequired();
            builder.Property(oi => oi.Quantity).IsRequired();
            builder.Property(oi => oi.Status).HasConversion<int>().IsRequired();

            builder.HasIndex(oi => oi.ProcessingOrderId);
            builder.HasIndex(oi => oi.ProductId);

            builder.HasOne<ProcessingOrder>()
                .WithMany(po => po.Items)
                .HasForeignKey(oi => oi.ProcessingOrderId)
                .OnDelete(DeleteBehavior.Cascade); ;
        }
    }
}
