using Microsoft.AspNetCore.Mvc;

namespace SkillNet.WebApi.Middleware
{
    public class InterviewExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InterviewExceptionMiddleware> _logger;

        public InterviewExceptionMiddleware(RequestDelegate next, ILogger<InterviewExceptionMiddleware> logger)
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
            catch (Exception exception) when (
                context.Request.Path.StartsWithSegments("/api/interviews") ||
                context.Request.Path.StartsWithSegments("/api/hiring"))
            {
                var status = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                    InvalidOperationException => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                if (status == StatusCodes.Status500InternalServerError)
                    _logger.LogError(exception, "Unhandled Interview API failure for {Path}.", context.Request.Path);

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = status,
                    Title = status switch
                    {
                        StatusCodes.Status400BadRequest => "Validation failed",
                        StatusCodes.Status403Forbidden => "Forbidden",
                        StatusCodes.Status404NotFound => "Resource not found",
                        StatusCodes.Status409Conflict => "Request conflict",
                        _ => "Server error"
                    },
                    Detail = status == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred while processing the interview request."
                        : exception.Message,
                    Instance = context.Request.Path
                });
            }
        }
    }
}
