using OrderProcessingService.Infrastructure.Extensions;
namespace OrderProcessingService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            string? dbconnection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructure(dbconnection!);
        }
    }
}
