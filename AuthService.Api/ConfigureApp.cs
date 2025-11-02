using AuthService.Infrastructure.Extensions;
using AuthService.Application.Extensions;
using Serilog;
using OrderManagementSystem.Shared.Middlewares;

namespace AuthService.Api
{
    public static class ConfigureApp
    {
        public static  IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure(configuration);
            services.AddApplication();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddControllers();
            return services;
        }

        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSerilogRequestLogging();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();
            app.MapControllers();
            return app;
        }
    }
}
