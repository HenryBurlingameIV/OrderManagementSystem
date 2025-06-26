using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Middlewares;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contracts;
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
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddMediatR(conf =>
            {
                conf.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
            });
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connection));
            var catalogConnection = builder.Configuration["CatalogService:DefaultConnection"];
            builder.Services.AddHttpClient("catalog", c =>
            {
                c.BaseAddress = new Uri(catalogConnection!);
            });
            builder.Services.AddTransient<ICatalogServiceClient, CatalogServiceClient>();
            builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
            builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.MapControllers();


            app.MapGet("/", () => "OrderService is running!");

            app.Run();
        }
    }
}
