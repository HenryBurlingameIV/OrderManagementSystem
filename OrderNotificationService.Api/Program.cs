using OrderNotificationService.Infrastructure.InfrastructureExtensions;
namespace OrderNotificationService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureSerilog();
            builder.ConfigureServices();
            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.RunDatabaseMigrations();
            }

            app.Run();
        }
    }
}
