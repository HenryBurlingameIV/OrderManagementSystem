using OrderNotificationService.Infrastructure.InfrastructureExtensions;

namespace OrderNotificationService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddInfrastructure(builder.Configuration);
        }
    }
}
