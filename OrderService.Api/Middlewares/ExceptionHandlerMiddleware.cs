using OrderService.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.ExceptionServices;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace OrderService.Api.Middlewares
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
            catch(ValidationException e)
            {
                await HandleException(context, HttpStatusCode.BadRequest, e.Message);
            }
            catch(HttpRequestException e)
            {
                var statusCode = e.StatusCode.HasValue ? e.StatusCode.Value : HttpStatusCode.BadRequest;
                await HandleException(context, statusCode, e.Message);
            }
            catch(NotFoundException e) 
            {
                await HandleException(context, HttpStatusCode.NotFound, e.Message);
            }
            catch(Exception e)
            {
                await HandleException(context, HttpStatusCode.InternalServerError, "Internal server error occured");
            }
        }

        public async Task HandleException(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(new {Message = message});
        }
    }
}
