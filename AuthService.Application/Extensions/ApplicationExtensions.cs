using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Contracts;
using FluentValidation;
using AuthService.Application.DTO;
using AuthService.Application.Validators;

namespace AuthService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserManagmentService, UserManagmentService>();
            services.AddScoped<IRoleProvider, RoleProvider>();
            services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
            services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
            services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
            return services;
        }
    }
}
