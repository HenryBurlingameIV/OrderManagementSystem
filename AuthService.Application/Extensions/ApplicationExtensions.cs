using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Contracts;

namespace AuthService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRoleProvider, RoleProvider>();
            return services;
        }
    }
}
