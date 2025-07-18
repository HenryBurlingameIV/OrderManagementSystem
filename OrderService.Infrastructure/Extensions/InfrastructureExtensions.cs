using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Contracts;
using OrderService.Domain.Entities;
using OrderService.Application.Contracts;
using OrderService.Infrastructure.HttpClients;
using OrderService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, string dbConnection, string catalogConnection) 
        {
            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseNpgsql(dbConnection);
            });

            services.AddHttpClient<ICatalogServiceApi, CatalogServiceApi>(conf =>
            {
                conf.BaseAddress = new Uri(catalogConnection);
            });



            services.AddScoped<IRepository<Order>, OrderRepository>();
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
