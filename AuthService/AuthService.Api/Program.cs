using AuthService.Infrastructure;
using AuthService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureSerilog();
            builder.Services.ConfigureServices(builder.Configuration);
            var app = builder.Build();
            app.ConfigurePipeline();

            if (!app.Environment.IsDevelopment())
            {
                app.RunDatabaseMigrations();
            }

            app.Run();
        }
    }
}
