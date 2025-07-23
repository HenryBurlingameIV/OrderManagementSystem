using Microsoft.Extensions.DependencyInjection;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrderProcessingInitializer, OrderProcessingInitializer>();
        }
    }
}
