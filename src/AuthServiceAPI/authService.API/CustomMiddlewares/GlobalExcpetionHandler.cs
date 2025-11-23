using System.Net;
using System.Text.Json;
using authService.Domain.CustomExceptions;

namespace authService.API.CustomMiddlewares;

public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
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
        string message = exception.Message;
        HttpStatusCode statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            DomainException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Error = message,
            Status = (int)statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}