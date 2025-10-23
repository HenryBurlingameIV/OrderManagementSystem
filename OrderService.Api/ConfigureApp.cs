using Microsoft.AspNetCore.Server.Kestrel.Core;
using OrderManagementSystem.Shared.Kafka;
using OrderManagementSystem.Shared.Middlewares;
using OrderService.Api.GrpcServices;
using OrderService.Application.DTO;
using OrderService.Application.Extensions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Extensions;
using Serilog;

namespace OrderService.Api
{
    public static class ConfigureApp
    {

        public static IWebHostBuilder ConfigureWebHost(this IWebHostBuilder webHostBuilder, IHostEnvironment env)
        {
            webHostBuilder.ConfigureKestrel(options =>
            {
                if (env.IsDevelopment())
                {
                    options.ListenLocalhost(5002, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                    options.ListenLocalhost(8081, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
                }
                else
                {
                    options.ListenAnyIP(5002, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                    options.ListenAnyIP(8081, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });
                }
            });
            return webHostBuilder;
        }
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddInfrastructure(configuration);
            services.AddApplication();
            services.AddProducer<OrderEvent>(configuration.GetSection("Kafka:OrderProducer"), "OrderProducer");
            services.AddProducer<OrderStatusEvent>(configuration.GetSection("Kafka:OrderStatusProducer"), "OrderStatusProducer");
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddGrpc();
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
            app.MapGrpcService<OrderGrpcService>();
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
