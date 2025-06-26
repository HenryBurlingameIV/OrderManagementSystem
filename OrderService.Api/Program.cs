using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contracts;
using OrderService.Infrastructure.HttpClients;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.MapControllers();


            app.MapGet("/", () => "OrderService is running!");

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
