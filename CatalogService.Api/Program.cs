using CatalogService.Api.Middlewares;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Extensions;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure.Extensions;
using CatalogService.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructure(connection!);
            builder.Services.AddControllers();
            builder.Services.AddApplication();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Host.UseSerilog();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSerilogRequestLogging();
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
