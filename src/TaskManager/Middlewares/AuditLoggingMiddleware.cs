using System.Text.Json;
using TaskManager.Services;

public class AuditLoggingMiddleware(RequestDelegate next, IConfiguration configuration)
{
  private readonly RequestDelegate _next = next;
  private readonly string[] _excludedPaths = ["/swagger", "/health"];
  private readonly bool _isEnabled = configuration.GetValue<bool>("AuditLogging:Enabled");

  public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
  {
    if (!_isEnabled)
    {
      await _next(context);
      return;
    }

    if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
    {
      await _next(context);
      return;
    }

    var username = context.User.Identity?.Name ?? "Anonymous";
    var action = $"{context.Request.Method} {context.Request.Path}";
    var details = new
    {
      context.Request.Headers,
      context.Request.Query
    };

    await _next(context);

    var responseDetails = new
    {
      context.Response.StatusCode
    };

    var combinedDetails = new
    {
      Request = details,
      Response = responseDetails
    };

    var formattedDetails = JsonSerializer.Serialize(combinedDetails, new JsonSerializerOptions
    {
      WriteIndented = true
    });

    await auditLogService.LogAsync(action, username, formattedDetails);
  }
}
