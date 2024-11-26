using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{

  private readonly HttpClient _client;

  public UsersControllerTests(CustomWebApplicationFactory<Program> factory) 
  {
    _client = factory.CreateClient();
  }
}