using CatalogService.Application.Contracts;
using CatalogService.Application.Services;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure.Repositories;
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
            builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.MapGet("/", () => "Hello World!");
            app.MapControllers();

            app.Run();
        }
    }
}
