using FluentValidation;
using Grpc.Core;
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
                ValidationException vEx => ((int)HttpStatusCode.BadRequest, vEx.Message),
                HttpRequestException httpEx => ((int)(httpEx.StatusCode ?? HttpStatusCode.BadRequest), httpEx.Message),
                NotFoundException notFoundEx => ((int)HttpStatusCode.NotFound, notFoundEx.Message),
                RpcException rpcEx => (MapGrpcStatusCode(rpcEx), rpcEx.Status.Detail),
                _ => ((int)HttpStatusCode.InternalServerError, ex.Message),
            };

            _logger.LogError("Error {@exception} occured", ex);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { Message = message });
        }

        private static int MapGrpcStatusCode(RpcException rpcEx)
        {
            return rpcEx.StatusCode switch
            {
                StatusCode.NotFound => 404,
                StatusCode.InvalidArgument or StatusCode.FailedPrecondition => 400,
                StatusCode.PermissionDenied => 403,
                StatusCode.Unauthenticated => 401,
                StatusCode.Unavailable => 503,
                _ => 500
            };
        }
    }
}
