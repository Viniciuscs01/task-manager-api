using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Services;

namespace TaskManager.Tests.Middlewares
{
  public class AuditLoggingMiddlewareTests
  {
    [Fact]
    public async Task InvokeAsync_ShouldLogAction_WhenAuditLoggingIsEnabled()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/api/tasks";
      context.User = new System.Security.Claims.ClaimsPrincipal(
          new System.Security.Claims.ClaimsIdentity(
          [
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser")
          ])
      );

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
      }, GetMockedConfiguration(true));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          "GET /api/tasks",
          "testuser",
          It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotLogAction_WhenAuditLoggingIsDisabled()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/api/tasks";

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
      }, GetMockedConfiguration(false));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipExcludedPaths()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/swagger";

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
      }, GetMockedConfiguration(true));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeAuthorizationHeaderInDetails()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/api/tasks";
      context.Request.Headers["Authorization"] = "Bearer test-token";

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
      }, GetMockedConfiguration(true));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          It.Is<string>(action => action.Contains("GET /api/tasks")),
          It.Is<string>(username => username == "Anonymous"),
          It.Is<string>(details => details.Contains("Bearer test-token"))), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogErrorResponse_WhenStatusCodeIs500()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/api/tasks";

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      }, GetMockedConfiguration(true));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          It.Is<string>(action => action.Contains("GET /api/tasks")),
          It.Is<string>(username => username == "Anonymous"),
          It.Is<string>(details => details.Contains("\"StatusCode\": 500"))), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeQueryParametersInDetails()
    {
      // Arrange
      var mockAuditLogService = new Mock<IAuditLogService>();
      var context = new DefaultHttpContext();
      context.Request.Method = "GET";
      context.Request.Path = "/api/tasks";
      context.Request.QueryString = new QueryString("?status=completed&priority=high");

      var middleware = new AuditLoggingMiddleware(async (innerHttpContext) =>
      {
        innerHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
      }, GetMockedConfiguration(true));

      // Act
      await middleware.InvokeAsync(context, mockAuditLogService.Object);

      // Assert
      mockAuditLogService.Verify(service => service.LogAsync(
          It.Is<string>(action => action.Contains("GET /api/tasks")),
          It.Is<string>(username => username == "Anonymous"),
          It.Is<string>(details => details.Contains("\"status\": \"completed\"") && details.Contains("\"priority\": \"high\""))), Times.Once);
    }
    private IConfiguration GetMockedConfiguration(bool isEnabled)
    {
      var inMemorySettings = new Dictionary<string, string>
            {
                {"AuditLogging:Enabled", isEnabled.ToString()}
            };

      return new ConfigurationBuilder()
          .AddInMemoryCollection(inMemorySettings)
          .Build();
    }
  }
}
