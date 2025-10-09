using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.DTO;
using OrderService.Application.Services;
using OrderService.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services )
        {
            services.AddMediatR(conf =>
            {
                conf.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
            });

            services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
            services.AddScoped<IValidator<OrderStatusValidationModel>, OrderStatusTransitionValidator>();
            services.AddScoped<IValidator<GetPaginatedOrdersRequest>, GetPaginatedOrdersRequestValidator>();
            services.AddScoped<OrderFactory>();
            return services;
        }
    }
}
