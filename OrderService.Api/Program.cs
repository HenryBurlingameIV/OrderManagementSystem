using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;

namespace OrderService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connection));
            var app = builder.Build();


            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
