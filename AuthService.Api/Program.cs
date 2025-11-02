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
            var dbConnection = builder.Configuration.GetConnectionString(nameof(AuthDbContext));
            builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(dbConnection);
            });
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
