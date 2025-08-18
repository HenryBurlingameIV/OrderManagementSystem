using OrderNotificationService.Infrastructure.InfrastructureExtensions;
using OrderNotificationService.Application.Extensions;

namespace OrderNotificationService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
        }
    }
}
