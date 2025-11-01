using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder
                .HasKey(r => r.Id);

            builder
                .Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder
                .HasMany(r => r.Permissions)
                .WithMany()
                .UsingEntity(j => j.ToTable("RolePermissions"));

            builder
                .HasIndex(r => r.Name)
                .IsUnique();
        }
    }
}
