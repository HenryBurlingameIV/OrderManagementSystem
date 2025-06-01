using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure.Repositories;
using FluentValidation;
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
            builder.Services.AddScoped<IValidator<ProductCreateRequest>, ProductCreateRequestValidator>();
            builder.Services.AddScoped<IValidator<ProductUpdateRequest>, ProductUpdateRequestValidator>();
            builder.Services.AddScoped<IValidator<ProductUpdateQuantityRequest>, ProductUpdateQuantityValidator>();
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
