using Hangfire;
using OrderManagementSystem.Shared.Kafka;
using OrderManagementSystem.Shared.Middlewares;
using OrderProcessingService.Application.Extensions;
using OrderProcessingService.Infrastructure.Extensions;
using Serilog;
namespace OrderProcessingService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
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
            app.MapGet("/", () => "OrderProcessingService is running!");
            app.UseHangfireDashboard("/hangfire");
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
