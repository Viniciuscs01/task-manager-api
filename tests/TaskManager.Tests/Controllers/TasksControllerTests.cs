using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TaskManager.Helpers;

public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client;
  private readonly string _jwtSecret;

  public TasksControllerTests(WebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();

    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    _jwtSecret = config["JWT_SECRET"];

    var token = AuthHelper.GenerateJwtToken(_jwtSecret, "testuser");
    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    _client.DefaultRequestHeaders.Add("Accept-Language", "en");
  }

  [Fact]
  public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
  {
    // Arrange
    var nonExistentId = 999;

    // Act
    var response = await _client.GetAsync($"/api/tasks/{nonExistentId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var content = await response.Content.ReadFromJsonAsync<JsonElement>();
    var error = content.GetProperty("error").GetString();

    error.Should().Be("Task not found.");
  }

  [Fact]
  public async Task CreateTask_ReturnsCreatedTask()
  {
    // Arrange
    var newTask = new TaskManager.Models.Task
    {
      Title = "Test Task",
      Description = "Test Description",
      IsCompleted = false,
      UserId = 1
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var createdTask = await response.Content.ReadFromJsonAsync<TaskManager.Models.Task>();
    createdTask.Should().NotBeNull();
    createdTask.Title.Should().Be("Test Task");
  }

  [Fact]
  public async Task GetTasks_WithPagination_ReturnsCorrectPage()
  {
    // Arrange
    var page = 1;
    var pageSize = 5;

    // Act
    var response = await _client.GetAsync($"/api/tasks?page={page}&pageSize={pageSize}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var content = await response.Content.ReadFromJsonAsync<JsonElement>();

    var actualPage = content.GetProperty("page").GetInt32();
    var actualPageSize = content.GetProperty("pageSize").GetInt32();
    var totalItems = content.GetProperty("totalItems").GetInt32();
    var items = content.GetProperty("items");

    actualPage.Should().Be(page);
    actualPageSize.Should().Be(pageSize);

    items.GetArrayLength().Should().BeLessThanOrEqualTo(pageSize);
  }

}