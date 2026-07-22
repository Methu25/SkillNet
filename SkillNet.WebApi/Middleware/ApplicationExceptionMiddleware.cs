using Microsoft.AspNetCore.Mvc;

namespace SkillNet.WebApi.Middleware
{
    /// <summary>
    /// Global exception handler for all /api/application routes.
    /// Maps known domain exception types to appropriate HTTP status codes.
    /// </summary>
    public class ApplicationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApplicationExceptionMiddleware> _logger;

        public ApplicationExceptionMiddleware(
            RequestDelegate next,
            ILogger<ApplicationExceptionMiddleware> logger)
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
            catch (Exception exception) when (context.Request.Path.StartsWithSegments("/api/application"))
            {
                var statusCode = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    InvalidOperationException => StatusCodes.Status409Conflict,
                    UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status500InternalServerError
                };

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(exception, "Unhandled Application API failure for {Path}.", context.Request.Path);
                }

                var detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred while processing the Application request."
                    : exception.Message;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = statusCode,
                    Title = statusCode switch
                    {
                        StatusCodes.Status400BadRequest => "Validation failed",
                        StatusCodes.Status404NotFound => "Resource not found",
                        StatusCodes.Status409Conflict => "Request conflict",
                        StatusCodes.Status403Forbidden => "Forbidden",
                        _ => "Server error"
                    },
                    Detail = detail,
                    Instance = context.Request.Path
                });
            }
        }
    }
}
