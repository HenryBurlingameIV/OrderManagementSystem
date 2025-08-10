using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrasrtucture.InfrastructureExtensions
{
    public static class InfrastructureExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string? dbconnection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OrderNotificationDbContext>(options =>
            {
                options.UseNpgsql(dbconnection);
            });
        }
    }
}
