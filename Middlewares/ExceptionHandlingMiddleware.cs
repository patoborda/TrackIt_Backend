using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using trackit.server.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace trackit.server.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly Dictionary<Type, HttpStatusCode> _exceptionStatusCodes = new()
        {
            { typeof(UserNotFoundException), HttpStatusCode.NotFound },
            { typeof(InvalidLoginException), HttpStatusCode.Unauthorized },
            { typeof(PasswordMismatchException), HttpStatusCode.BadRequest },
            { typeof(UserCreationException), HttpStatusCode.BadRequest },
            { typeof(IncompleteUserInfoException), HttpStatusCode.BadRequest },
            { typeof(JwtKeyNotConfiguredException), HttpStatusCode.InternalServerError },
            { typeof(JwtGenerationException), HttpStatusCode.InternalServerError },
            { typeof(PasswordResetException), HttpStatusCode.BadRequest },
            { typeof(EmailSendException), HttpStatusCode.InternalServerError },
            { typeof(UserProfileException), HttpStatusCode.BadRequest }
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (_exceptionStatusCodes.TryGetValue(exception.GetType(), out var statusCode))
            {
                context.Response.StatusCode = (int)statusCode;
                var result = new { error = new { code = exception.GetType().Name, message = exception.Message } };
                return context.Response.WriteAsJsonAsync(result);
            }

            // Log the unexpected errors
            _logger.LogError(exception, "An unexpected error occurred.");

            // Handle generic exceptions
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var errorResult = new { error = new { code = "INTERNAL_SERVER_ERROR", message = "An unexpected error occurred." } };
            return context.Response.WriteAsJsonAsync(errorResult);
        }


    }
}
