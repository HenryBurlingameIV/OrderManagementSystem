using CatalogService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CatalogService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")!));
            services.AddScoped<IEFRepository<Product, Guid>>(provider =>
            {
                var dbContext = provider.GetRequiredService<CatalogDbContext>();
                return new Repository<Product, Guid>(dbContext);
            });

            return services;
        }

        public static void RunDatabaseMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    db.Database.Migrate();
                }
            }
        }
    }
}
