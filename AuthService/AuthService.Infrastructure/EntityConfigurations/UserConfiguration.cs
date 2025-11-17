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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(user => user.Id);

            builder
                .Property(user => user.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder
                .Property(user => user.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder
                .Property(user => user.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder
                .Property(user => user.HashedPassword)
                .HasMaxLength(254)
                .IsRequired();

            builder
                .HasMany(user => user.Roles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    j => j.HasData(SeedData.GetUserRolesForSeed()));

            builder
                .HasIndex(user => user.Email)
                .IsUnique();

            builder
                .HasIndex(user => user.Name);

            builder
                .HasData(SeedData.GetAdminUser());
        }
    }
}
