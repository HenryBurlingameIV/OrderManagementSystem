using OrderProcessingService.Infrastructure.Extensions;
using Serilog;
namespace OrderProcessingService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            string? dbconnection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructure(dbconnection!);
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
