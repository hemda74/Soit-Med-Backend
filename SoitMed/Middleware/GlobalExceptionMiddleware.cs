using System.Net;
using System.Text.Json;
using SoitMed.Common;
using SoitMed.Common.Exceptions;

namespace SoitMed.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "An internal server error occurred",
                errorCode = (string?)null,
                data = (object?)null
            };

            switch (exception)
            {
                case InvalidStateTransitionException stateEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new 
                    { 
                        success = false, 
                        message = stateEx.Message, 
                        errorCode = "INVALID_STATE_TRANSITION",
                        data = (object?)new 
                        { 
                            currentState = stateEx.CurrentState,
                            attemptedState = stateEx.AttemptedState,
                            entityName = stateEx.EntityName
                        }
                    };
                    break;

                case SecurityException securityEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response = new 
                    { 
                        success = false, 
                        message = securityEx.Message, 
                        errorCode = "SECURITY_VIOLATION",
                        data = (object?)new 
                        { 
                            operation = securityEx.Operation,
                            resourceId = securityEx.ResourceId
                        }
                    };
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { success = false, message = argEx.Message, errorCode = "INVALID_ARGUMENT", data = (object?)null };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response = new { success = false, message = "Access denied", errorCode = "UNAUTHORIZED", data = (object?)null };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new { success = false, message = "Resource not found", errorCode = "NOT_FOUND", data = (object?)null };
                    break;

                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { success = false, message = invalidOpEx.Message, errorCode = "INVALID_OPERATION", data = (object?)null };
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = new { success = false, message = "Request timeout", errorCode = "TIMEOUT", data = (object?)null };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new { success = false, message = "An unexpected error occurred", errorCode = "INTERNAL_ERROR", data = (object?)null };
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
