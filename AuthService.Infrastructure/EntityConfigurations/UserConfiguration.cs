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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(user => user.Id);

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
                .UsingEntity(j => j.ToTable("UserRoles"));

            builder
                .HasIndex(user => user.Email)
                .IsUnique();

            builder
                .HasIndex(user => user.Name);                        
        }
    }
}
