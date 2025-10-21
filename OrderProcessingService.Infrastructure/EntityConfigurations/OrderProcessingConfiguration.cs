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
    public class ProcessingOrderConfiguration : IEntityTypeConfiguration<ProcessingOrder>
    {
        public void Configure(EntityTypeBuilder<ProcessingOrder> builder)
        {
            builder
                .HasKey(po => po.Id);
            builder
                .Property(po => po.OrderId)
                .IsRequired();
            builder
                .HasIndex(po => po.OrderId)
                .IsUnique();

            builder
                .HasMany(po => po.Items)
                .WithOne()
                .HasForeignKey(item => item.ProcessingOrderId)
                .IsRequired();

            builder
                .Property (po => po.Stage)
                .HasConversion<int>()
                .IsRequired();

            builder
                .HasIndex(po => po.Stage);


            builder
                .Property(po => po.Status)
                .HasConversion<int>()
                .IsRequired();

            builder
                .HasIndex(po => po.Status);

            builder
                .Property(po => po.CreatedAt)
                .IsRequired();


            builder
                .Property(po => po.UpdatedAt)
                .IsRequired();

            builder
                .Property(po => po.TrackingNumber)
                .HasMaxLength(50)
                .IsRequired(false);
        }
    }
}
