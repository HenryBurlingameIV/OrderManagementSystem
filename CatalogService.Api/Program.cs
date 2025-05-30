using CatalogService.Domain;
using CatalogService.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<CatalogDBContext>(options => options.UseSqlServer(connection));
            builder.Services.AddControllers();
            var app = builder.Build();


            app.MapGet("/", () => "Hello World!");
            app.MapControllers();

            app.Run();
        }
    }
}
