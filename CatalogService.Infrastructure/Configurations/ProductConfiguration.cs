using CatalogService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace CatalogService.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.HasIndex(p => p.Name)
                .IsUnique();
            builder.Property(p => p.Description)
                .HasMaxLength(200);
            builder.HasIndex(p => p.Category);
            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
