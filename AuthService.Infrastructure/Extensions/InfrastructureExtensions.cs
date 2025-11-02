using AuthService.Application;
using AuthService.Application.Contracts;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(AuthDbContext)));
            });

            services.AddScoped<IEFRepository<User, Guid>, Repository<User, Guid>>(rpovider =>
            {
                var context = rpovider.GetRequiredService<AuthDbContext>();
                return new Repository<User, Guid>(context);
            });

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.AddScoped<IJwtProvider, JwtProvider>();

            return services;
        }

    }
}
