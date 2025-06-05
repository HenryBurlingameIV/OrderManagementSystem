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
            builder.ConfigureServices();
            builder.ConfigureSerilog();

            var app = builder.Build();
            app.ConfigurePipeline();

            app.Run();
        }
    }
}
