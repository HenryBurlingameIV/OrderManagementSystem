using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IValidator<ProductCreateRequest>, ProductCreateRequestValidator>();
            services.AddScoped<IValidator<ProductUpdateRequest>, ProductUpdateRequestValidator>();
            services.AddScoped<IValidator<ProductUpdateQuantityRequest>, ProductUpdateQuantityValidator>();
            return services;
        }
    }
}
