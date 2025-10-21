using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Application.Services;
using OrderProcessingService.Application.Validators;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrderProcessingInitializer, OrderProcessingInitializer>();
            services.AddScoped<IOrderProcessor, OrderProcessor>();
            services.AddScoped<IProcessingOrderQueryService, ProcessingOrderQueryService>();
            services.AddScoped<IValidator<StartAssemblyStatus>, StartAssemblyValidator>();
            services.AddScoped<IValidator<StartDeliveryStatus>, StartDeliveryValidator>();
            services.AddScoped<IValidator<GetPaginatedProcessingOrdersRequest>, GetPaginatedProcessingOrderRequestValidator>();
            return services;
        }
    }
}
