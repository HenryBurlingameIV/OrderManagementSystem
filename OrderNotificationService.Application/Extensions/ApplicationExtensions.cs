using Microsoft.Extensions.DependencyInjection;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Application.DTO;
using OrderNotificationService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IMessageTemplateRenderer, MessageTemplateRenderer>();
            services.AddScoped<INotificationService<NotificationRequest>, NotificationService>();
        }
    }
}
