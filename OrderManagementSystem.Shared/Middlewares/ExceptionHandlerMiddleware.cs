using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Exceptions;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.ExceptionServices;
using ValidationException = FluentValidation.ValidationException;

namespace OrderManagementSystem.Shared.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger) 
        {
            _next = next;
            _logger = logger;
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
                HttpRequestException httpEx => (httpEx.StatusCode ?? HttpStatusCode.BadRequest, httpEx.Message),
                NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message),
                _ => (HttpStatusCode.InternalServerError, ex.Message),
            };

            _logger.LogError("Error {@exception} occured", ex);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(new { Message = message });
        }
    }
}
