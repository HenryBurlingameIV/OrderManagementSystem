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
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddInfrastructure(configuration);
            services.AddApplication();
            services.AddProducer<OrderEvent>(configuration.GetSection("Kafka:OrderProducer"), "OrderProducer");
            services.AddProducer<OrderStatusEvent>(configuration.GetSection("Kafka:OrderStatusProducer"), "OrderStatusProducer");
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();
            app.MapControllers();
            return app;
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
