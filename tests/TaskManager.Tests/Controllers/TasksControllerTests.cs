using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Helpers;
using TaskManager.Models;
using TaskManager.Tests.Controllers;

public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;
  private readonly string _jwtSecret;
  private readonly ApplicationDbContext _context;
  private readonly CustomWebApplicationFactory<Program> _factory;

  public TasksControllerTests(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _client = factory.CreateClient();

    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    _jwtSecret = config["JWT_SECRET"];

    var token = AuthHelper.GenerateJwtToken(_jwtSecret, "testuser");
    _client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    _client.DefaultRequestHeaders.Add("Accept-Language", "en");

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TaskManagerTestDb")
                .Options;

    _context = new ApplicationDbContext(options);
  }

  [Fact]
  public async System.Threading.Tasks.Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
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
  public async System.Threading.Tasks.Task CreateTask_ReturnsCreatedTask()
  {
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Adicionar um usuário válido ao banco de dados
    var validUser = new User { Id = 1, Username = "Test User", Email = "test@example.com", PasswordHash = "hashed_password" };
    context.Users.Add(validUser);
    await context.SaveChangesAsync();

    var newTask = new
    {
      Title = "Test Task",
      Description = "Test Description",
      IsCompleted = false,
      UserId = validUser.Id // Associar ao usuário válido
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var createdTask = await response.Content.ReadFromJsonAsync<TaskManager.Models.Task>();
    createdTask.Should().NotBeNull();
    createdTask.Title.Should().Be("Test Task");
    createdTask.UserId.Should().Be(validUser.Id);
  }


  [Fact]
  public async System.Threading.Tasks.Task GetTasks_WithPagination_ReturnsCorrectPage()
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
    _ = content.GetProperty("totalItems").GetInt32();
    var items = content.GetProperty("items");

    actualPage.Should().Be(page);
    actualPageSize.Should().Be(pageSize);

    items.GetArrayLength().Should().BeLessThanOrEqualTo(pageSize);
  }

  [Fact]
  public async System.Threading.Tasks.Task GetTasks_ShouldFilterByStatus()
  {
    // Arrange
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var completedTask = new TaskManager.Models.Task { Title = "Completed Task", Description = "Completed Task", IsCompleted = true };
    var pendingTask = new TaskManager.Models.Task { Title = "Pending Task", Description = "Pending Task", IsCompleted = false };

    context.Tasks.AddRange(completedTask, pendingTask);
    await context.SaveChangesAsync();

    // Act
    var response = await _client.GetAsync("/api/tasks?isCompleted=true");
    var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<TaskManager.Models.Task>>();

    // Assert
    wrapper.Should().NotBeNull();
    wrapper.Items.Should().HaveCount(1);
    wrapper.Items.First().Title.Should().Be("Completed Task");
  }

  [Fact]
  public async System.Threading.Tasks.Task GetTasks_ShouldFilterByDateRange()
  {
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Tasks.AddRange(
        new TaskManager.Models.Task { Title = "Task 1", Description = "Task 1", CreatedAt = new DateTime(2024, 11, 01) },
        new TaskManager.Models.Task { Title = "Task 2", Description = "Task 2", CreatedAt = new DateTime(2024, 11, 10) },
        new TaskManager.Models.Task { Title = "Task 3", Description = "Task 3", CreatedAt = new DateTime(2024, 11, 20) }
    );

    await context.SaveChangesAsync();

    // Act
    var response = await _client.GetAsync("/api/tasks?startDate=2024-11-01&endDate=2024-11-15");
    var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<TaskManager.Models.Task>>();

    // Assert
    wrapper.Should().NotBeNull();
    wrapper.Items.Should().HaveCount(2);
    wrapper.Items.Select(t => t.Title).Should().Contain(new[] { "Task 1", "Task 2" });
  }

  [Fact]
  public async System.Threading.Tasks.Task UpdateTask_ShouldReturnUpdatedTask()
  {
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var existingTask = new TaskManager.Models.Task
    {
      Title = "Old Task",
      Description = "Old Description",
      IsCompleted = false,
      UserId = 1
    };

    context.Tasks.Add(existingTask);
    await context.SaveChangesAsync();

    var updatedTask = new
    {
      existingTask.Id,
      Title = "Updated Task",
      Description = "Updated Description",
      IsCompleted = true
    };

    // Act
    var response = await _client.PutAsJsonAsync($"/api/tasks/{existingTask.Id}", updatedTask);
    var result = await response.Content.ReadFromJsonAsync<TaskManager.Models.Task>();

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    result.Should().NotBeNull();
    result.Title.Should().Be("Updated Task");
    result.Description.Should().Be("Updated Description");
    result.IsCompleted.Should().BeTrue();
  }
  [Fact]
  public async System.Threading.Tasks.Task UpdateTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
  {
    // Arrange
    var updatedTask = new
    {
      Id = 999,
      Title = "Updated Task",
      Description = "Updated Description",
      IsCompleted = true
    };

    // Act
    var response = await _client.PutAsJsonAsync("/api/tasks/999", updatedTask);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async System.Threading.Tasks.Task UpdateTask_ShouldReturnBadRequest_WhenIdMismatch()
  {
    // Arrange
    var updatedTask = new
    {
      Id = 1,
      Title = "Updated Task",
      Description = "Updated Description",
      IsCompleted = true
    };

    // Act
    var response = await _client.PutAsJsonAsync("/api/tasks/2", updatedTask);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async System.Threading.Tasks.Task DeleteTask_ShouldReturnNoContent_WhenTaskIsDeleted()
  {
    // Arrange
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var task = new TaskManager.Models.Task
    {
      Title = "Test Task",
      Description = "Test Description",
      IsCompleted = false,
      UserId = 1
    };

    context.Tasks.Add(task);
    await context.SaveChangesAsync();

    // Act
    var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    context.Entry(task).State = EntityState.Detached;

    var deletedTask = await context.Tasks.FindAsync(task.Id);
    deletedTask.Should().BeNull();
  }

  [Fact]
  public async System.Threading.Tasks.Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
  {
    // Act
    var response = await _client.DeleteAsync("/api/tasks/999");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}