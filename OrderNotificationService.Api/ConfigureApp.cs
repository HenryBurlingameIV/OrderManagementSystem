using OrderNotificationService.Application.Extensions;
using OrderNotificationService.Infrastructure.InfrastructureExtensions;
using Serilog;

namespace OrderNotificationService.Api
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
