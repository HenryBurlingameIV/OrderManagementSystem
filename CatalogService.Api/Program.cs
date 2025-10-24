using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Extensions;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
                .ConfigureWebHost(builder.Environment, builder.Configuration);


            builder.Services
                .ConfigureServices(builder.Configuration);

            builder.ConfigureSerilog();

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
