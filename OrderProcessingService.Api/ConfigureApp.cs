using OrderManagementSystem.Shared.Kafka;
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
