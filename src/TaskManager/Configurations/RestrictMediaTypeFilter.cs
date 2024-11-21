using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskManager.Configurations
{
  public class RestrictMediaTypeFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      foreach (var response in operation.Responses)
      {
        response.Value.Content = response.Value.Content
            .Where(c => c.Key == "application/json")
            .ToDictionary(c => c.Key, c => c.Value);
      }
    }
  }
}
