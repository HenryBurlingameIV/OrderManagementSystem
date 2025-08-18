using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Kafka;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Infrastructure.Messaging.Handlers;
using OrderNotificationService.Infrastructure.Messaging.Messages;
using OrderNotificationService.Infrastructure.Repositories;
using OrderNotificationService.Infrastructure.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrastructure.InfrastructureExtensions
{
    public static class InfrastructureExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string? dbconnection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OrderNotificationDbContext>(options =>
            {
                options.UseNpgsql(dbconnection);
            });

            services.AddScoped<INotificationTemplatesRepository, NotificationTemplatesRepository>();
            services.AddScoped<IEmailMessageSender, SmptEmailSender>();

            var kafkaConsumerConf = configuration.GetSection("Kafka:OrderStatusConsumer");
            services.AddConsumer<OrderStatusChangedMessage, OrderStatusChangedMessageHandler>(kafkaConsumerConf);
        }

        public static void RunDatabaseMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrderNotificationDbContext>();
                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    db.Database.Migrate();
                }
            }
        }
    }
}
