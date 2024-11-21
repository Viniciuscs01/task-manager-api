using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskManager.Configurations
{
  public class SwaggerExamplesFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      foreach (var response in operation.Responses)
      {
        if (response.Key == "200" && response.Value.Content.ContainsKey("application/json"))
        {
          response.Value.Content["application/json"].Example = new OpenApiObject
          {
            ["id"] = new OpenApiInteger(1),
            ["title"] = new OpenApiString("Sample Task"),
            ["description"] = new OpenApiString("This is a sample task."),
            ["isCompleted"] = new OpenApiBoolean(false)
          };
        }

        if (response.Key == "404" && response.Value.Content.ContainsKey("application/json"))
        {
          response.Value.Content["application/json"].Example = new OpenApiObject
          {
            ["error"] = new OpenApiString("Task not found.")
          };
        }
      }
    }
  }
}
