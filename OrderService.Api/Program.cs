using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contracts;
using OrderService.Infrastructure.HttpClients;

namespace OrderService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connection));
            var catalogConnection = builder.Configuration["CatalogService:DefaultConnection"];
            builder.Services.AddHttpClient("catalog", c =>
            {
                c.BaseAddress = new Uri(catalogConnection!);
            });

            builder.Services.AddTransient<ICatalogServiceClient, CatalogServiceClient>();
            var app = builder.Build();


            app.MapGet("/", () => "Hello World!");

            //using(var scope = app.Services.CreateScope())
            //{
            //    Task.Delay(1000).Wait();
            //    var client = app.Services.GetService<ICatalogServiceClient>();
            //    var id = Guid.Parse("1ec6a745-1978-45a5-b052-476d15f6bdff");
            //    var r = client.GetProductByIdAsync(id, new CancellationToken()).Result;

            //    Console.Out.WriteLineAsync($"Price: {r.Price}!!!!!");
            //}

            app.Run();
        }
    }
}
