using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.EntityConfigurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder
                .HasKey(p => p.Id);

            builder
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder
                .HasIndex(p => p.Name)
                .IsUnique();

            builder
                .HasData(SeedData.GetPermissionsForSeed());
        }
    }
}
