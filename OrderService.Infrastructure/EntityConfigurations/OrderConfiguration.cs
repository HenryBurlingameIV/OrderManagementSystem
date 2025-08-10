using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.OwnsMany(o => o.Items, item =>
            {
                item.ToJson();
                item.Property(i => i.ProductId).IsRequired();
                item.Property(i => i.Quantity).IsRequired();
                item.Property(i => i.Price).IsRequired();
            });

            builder.HasIndex(o => o.Status);

            builder.HasIndex(o => o.CreatedAtUtc);

            builder.HasIndex(o => new { o.Status, o.CreatedAtUtc });

            builder.Property(o => o.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .HasConversion<int>()
                .IsRequired();
            
            builder.Property(o => o.CreatedAtUtc) 
                .IsRequired();
            builder.Property(o => o.UpdatedAtUtc) 
                .IsRequired();

            builder
                .Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(50);

        }

    }
}
