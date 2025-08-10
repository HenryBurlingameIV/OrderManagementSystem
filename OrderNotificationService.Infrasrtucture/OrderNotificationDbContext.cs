using Microsoft.EntityFrameworkCore;
using OrderNotificationService.Domain.Entities;
using OrderNotificationService.Infrasrtucture.EntityConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrasrtucture
{
    public class OrderNotificationDbContext : DbContext
    {
        public OrderNotificationDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
        }

    }
}
