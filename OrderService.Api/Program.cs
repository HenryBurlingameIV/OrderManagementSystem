using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contracts;
using OrderService.Infrastructure.Extensions;
using OrderService.Infrastructure.HttpClients;
using OrderService.Infrastructure.Repositories;
using Serilog;

namespace OrderService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureSerilog();
            builder.ConfigureServices();
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
