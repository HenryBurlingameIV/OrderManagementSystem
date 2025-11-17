using Hangfire;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Kafka;
using OrderManagementSystem.Shared.Middlewares;
using OrderProcessingService.Application.Extensions;
using OrderProcessingService.Infrastructure.Extensions;
using Serilog;
namespace OrderProcessingService.Api
{
    public static class ConfigureApp
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddJwtAuthentication(configuration);
            services.AddPermissionAuthorization();
            services.AddApplication();
            services.AddInfrastructure(configuration);
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;

        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseHangfireDashboard("/hangfire");
            return app;
        }

        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });
        }
    }
}
