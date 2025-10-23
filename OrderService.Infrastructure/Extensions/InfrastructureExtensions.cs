using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Contracts;
using OrderService.Domain.Entities;
using OrderService.Application.Contracts;
using OrderService.Infrastructure.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderManagementSystem.Shared.DataAccess;
using Microsoft.Extensions.Configuration;
using CatalogService.Api.Protos;

namespace OrderService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) 
        {

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("OrderDbContext"));
            });

            services.AddScoped<IEFRepository<Order, Guid>>(provider =>
            {
                var context = provider.GetRequiredService<OrderDbContext>();
                return new Repository<Order, Guid>(context);
            });


            services.AddScoped<IRepositoryBase<Order, Guid>>(provider =>
            {
                var context = provider.GetRequiredService<OrderDbContext>();
                return new Repository<Order, Guid>(context);
            });

            //services.AddHttpClient<ICatalogServiceApi, CatalogServiceApi>(conf =>
            //{
            //    conf.BaseAddress = new Uri(
            //        configuration["CatalogService:HttpConnection"]!);
            //});
            services.AddScoped<ICatalogServiceApi, CatalogGrpcClient>();
            services.AddGrpcClient<Catalog.CatalogClient>(options =>
            {
                options.Address = new Uri(
                    configuration["CatalogService:GrpcConnection"]!);
            });

            return services;
        }

        public static void RunDatabaseMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    db.Database.Migrate();
                }
            }
        }
    }
}
