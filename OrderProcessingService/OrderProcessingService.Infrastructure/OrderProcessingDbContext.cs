using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Infrastructure.EntityConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure
{
    public class OrderProcessingDbContext : DbContext
    {
        public OrderProcessingDbContext(DbContextOptions options) : base (options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProcessingOrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        }

        public DbSet<ProcessingOrder> ProcessingOrders { get; set; }   
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
