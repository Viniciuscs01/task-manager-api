using System.Net;
using System.Text.Json;

namespace TaskManager.Middlewares
{
  public class ErrorHandlingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger)
    {
      _next = next;
      _env = env;
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
        _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
        await HandleExceptionAsync(context, ex);
      }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
      var statusCode = HttpStatusCode.InternalServerError;

      if (exception is UnauthorizedAccessException)
        statusCode = HttpStatusCode.Unauthorized;

      var response = new
      {
        error = exception.Message,
        statusCode = (int)statusCode,
        stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
      };

      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)statusCode;

      return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
  }
}
