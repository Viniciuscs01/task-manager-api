using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace TaskManager.Middlewares
{
  public class ErrorHandlingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ErrorHandlingMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger, IStringLocalizer<SharedResource> localizer)
    {
      _next = next;
      _env = env;
      _logger = logger;
      _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      try
      {
        await _next(context);
      }
      catch (Exception ex)
      {
        var statusCode = HttpStatusCode.InternalServerError;
        var localizedMessage = _localizer["ValidationError"].Value;
        
        _logger.LogError(ex, localizedMessage, ex.Message);

        var response = new
        {
          error = localizedMessage,
          statusCode = (int)statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
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
