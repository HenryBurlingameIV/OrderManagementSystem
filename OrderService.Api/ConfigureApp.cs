using OrderManagementSystem.Shared.Kafka;
using OrderManagementSystem.Shared.Middlewares;
using OrderService.Application.DTO;
using OrderService.Application.Extensions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Extensions;
using Serilog;

namespace OrderService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection");
            var catalogConnection = builder.Configuration["CatalogService:DefaultConnection"];
            builder.Services.AddInfrastructure(dbConnection!, catalogConnection!);
            builder.Services.AddApplication();
            builder.Services.AddProducer<OrderEvent>(builder.Configuration.GetSection("Kafka:OrderProducer"), "OrderProducer");
            builder.Services.AddProducer<OrderStatusEvent>(builder.Configuration.GetSection("Kafka:OrderStatusProducer"), "OrderStatusProducer");
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        public static void ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.MapControllers();
            app.MapGet("/", () => "OrderService is running!");
        }

        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });
        }
    }
}
