using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using CatalogService.Infrastructure.Configurations;

namespace CatalogService.Infrastructure
{
    public class CatalogDBContext : DbContext
    {
        public CatalogDBContext(DbContextOptions options) : base (options){ }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }
        DbSet<Product> Products { get; set; }

    }
}

    
