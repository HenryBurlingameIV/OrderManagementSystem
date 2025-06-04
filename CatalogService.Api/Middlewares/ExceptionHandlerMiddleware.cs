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
            catch(ValidationException e)
            {
                await HandleException(context, HttpStatusCode.BadRequest, e);
            }
            catch(NotFoundException e)
            {
                await HandleException(context, HttpStatusCode.NotFound, e);
            }
            catch(Exception e)
            {
                await HandleException(context, HttpStatusCode.InternalServerError, e);
            }
        }

        public async Task HandleException(HttpContext context, HttpStatusCode httpStatusCode, Exception e)
        {
            Log.Error("Error {@exception} occured", e);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)httpStatusCode;
            await context.Response.WriteAsJsonAsync
                (
                    new 
                    { 
                        Message = e.Message, 
                        StatusCode = (int)httpStatusCode 
                    }
                );
        }
    }
}
