using CatalogService.Domain;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnection)
        {
            services.AddDbContext<CatalogDBContext>(options => options.UseNpgsql(dbConnection));
            services.AddScoped<IRepository<Product>, ProductRepository>();

            return services;
        }
    }
}
