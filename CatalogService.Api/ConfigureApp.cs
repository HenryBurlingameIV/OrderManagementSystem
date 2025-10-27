using CatalogService.Api.GrpcServices;
using CatalogService.Application.Extensions;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Extensions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrderManagementSystem.Shared.Middlewares;
using Serilog;

namespace CatalogService.Api
{
    public static class ConfigureApp
    {
        public static IWebHostBuilder ConfigureWebHost(this IWebHostBuilder webHostBuilder, IConfiguration configuration)
        {
            webHostBuilder.ConfigureKestrel(options =>
            {                
                var grpcPort = configuration.GetValue<int>("Endpoints:GrpcPort");                
                options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
                var restPort = configuration.GetValue<int>("Endpoints:RestPort");
                options.ListenAnyIP(restPort, o => o.Protocols = HttpProtocols.Http1);

            });
            return webHostBuilder;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure(configuration);
            services.AddControllers();
            services.AddApplication();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddGrpc();
            return services;
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
            app.MapGrpcService<CatalogGrpcService>();
            app.MapControllers();
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
