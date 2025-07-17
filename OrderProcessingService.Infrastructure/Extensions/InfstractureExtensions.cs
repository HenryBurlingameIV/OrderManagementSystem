using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Extensions
{
    public static class InfstractureExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, string dbconnection)
        {
            services.AddDbContext<OrderProcessingDbContext>(options =>
            {
                options.UseNpgsql(dbconnection);
            });
        }
    }
}
