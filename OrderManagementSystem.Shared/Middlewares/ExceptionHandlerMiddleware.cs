using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.ExceptionServices;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;
using Microsoft.AspNetCore.Http;
using Serilog;
using OrderManagementSystem.Shared.Exceptions;

namespace OrderManagementSystem.Shared.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        RequestDelegate _next;
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
                HttpRequestException httpEx => (httpEx.StatusCode ?? HttpStatusCode.BadRequest, httpEx.Message),
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
