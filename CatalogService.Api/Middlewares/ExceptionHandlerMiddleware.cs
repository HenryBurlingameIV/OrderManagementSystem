using CatalogService.Domain.Exceptions;
using FluentValidation;
using Serilog;
using System.Net;

namespace CatalogService.Api.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        public async Task HandleException(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                ValidationException vEx => (HttpStatusCode.BadRequest, vEx.Message),
                NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message),
                _ => (HttpStatusCode.InternalServerError, ex.Message),
            };
            Log.Error("Error {@exception} occured", ex);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(new { Message = message });
        }
    }
}
