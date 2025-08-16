using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Kafka;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Infrastructure.BackgroundWorkers;
using OrderProcessingService.Infrastructure.ExternalServices;
using OrderProcessingService.Infrastructure.Messaging.Handlers;
using OrderProcessingService.Infrastructure.Messaging.Messages;
using OrderProcessingService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string? dbconnection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OrderProcessingDbContext>(options =>
            {
                options.UseNpgsql(dbconnection);
            });
            services.AddScoped<IRepository<ProcessingOrder>, ProcessingOrderRepository>();
            services.AddScoped<IProcessingOrderRepository, ProcessingOrderRepository>();
            var kafkaConf = configuration.GetSection("Kafka:Order");
            services.AddConsumer<OrderCreatedMessage, OrderCreatedMessageHandler>(kafkaConf);
            services.AddHttpClient<IOrderServiceApi, OrderServiceApi>(conf =>
            {
                conf.BaseAddress = new Uri(configuration["OrderService:DefaultConnection"]!);
            });
            string hangfireStorageConnection = configuration.GetConnectionString("HangfireConnection")!;
            services.AddHangfire(conf => 
                conf.UsePostgreSqlStorage(options => 
                    options.UseNpgsqlConnection(hangfireStorageConnection)));

            services.AddHangfireServer();

            services.AddScoped<IOrderBackgroundWorker<StartAssemblyCommand>, AssemblyWorker>();
            services.AddScoped<IOrderBackgroundWorker<StartDeliveryCommand>, DeliveryWorker>();
            
        }

        public static void RunDatabaseMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrderProcessingDbContext>();
                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    db.Database.Migrate();
                }
            }
        }
    }
}
