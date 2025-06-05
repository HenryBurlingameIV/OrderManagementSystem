using CatalogService.Api.Middlewares;
using CatalogService.Application.Extensions;
using CatalogService.Infrastructure.Extensions;
using Serilog;

namespace CatalogService.Api
{
    public static class ConfigureApp
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddInfrastructure(connection!);
            builder.Services.AddControllers();
            builder.Services.AddApplication();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        public static void ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => "Hello World!");
            app.MapControllers();
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
