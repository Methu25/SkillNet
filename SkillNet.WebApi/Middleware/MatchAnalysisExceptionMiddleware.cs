using Microsoft.AspNetCore.Mvc;

namespace SkillNet.WebApi.Middleware;

public class MatchAnalysisExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception) when (context.Request.Path.StartsWithSegments("/api/match-analysis"))
        {
            var status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = status == 500 ? "Server error" : "Match analysis request failed",
                Detail = status == 500 ? "The match analysis could not be completed." : exception.Message,
                Instance = context.Request.Path
            });
        }
    }
}
