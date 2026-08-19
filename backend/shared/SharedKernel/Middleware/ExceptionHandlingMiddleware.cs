using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace SharedKernel.Middleware;

/// <summary>
/// Global exception-handling middleware that catches domain exceptions
/// and maps them to consistent HTTP error responses.
/// Registered in every service's Program.cs via app.UseMiddleware().
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message),

            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, validationEx.Message),

            UnauthorizedAccessException unauthorizedEx =>
                (HttpStatusCode.Forbidden, unauthorizedEx.Message),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception for {Method} {Path}: {ExceptionType} - {Message}",
                context.Request.Method, context.Request.Path,
                exception.GetType().Name, message);
        }

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
