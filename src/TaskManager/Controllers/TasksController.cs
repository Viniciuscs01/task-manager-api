using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TaskManager.Models;

namespace TaskManager.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class TasksController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TasksController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger, IStringLocalizer<SharedResource> localizer)
    {
      _context = context;
      _logger = logger;
      _localizer = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] Models.Task task)
    {
      _logger.LogInformation("Creating a new task with title: {Title}", task.Title);

      if (!_context.Users.Any(u => u.Id == task.UserId))
      {
        return BadRequest("Usuário inválido.");
      }

      _context.Tasks.Add(task);
      await _context.SaveChangesAsync();

      _logger.LogInformation("Task created successfully with ID: {Id}", task.Id);

      return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Retrieve a specific task by ID.
    /// </summary>
    /// <param name="id">ID of the task.</param>
    /// <returns>Task details or not found response.</returns>
    /// <response code="200">Returns the task details.</response>
    /// <response code="404">Task not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Models.Task), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaskById(int id)
    {
      _logger.LogInformation("Fetching task with ID: {Id}", id);

      var task = await _context.Tasks.FindAsync(id);
      if (task == null)
      {
        var message = _localizer["TaskNotFound"].Value;
        _logger.LogWarning(message);
        return NotFound(new { error = message });
      }

      _logger.LogInformation("Task with ID {Id} fetched successfully", id);
      return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, Models.Task task)
    {
      if (id != task.Id)
      {
        return BadRequest();
      }

      _context.Entry(task).State = EntityState.Modified;
      await _context.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task == null)
      {
        return NotFound();
      }

      _context.Tasks.Remove(task);
      await _context.SaveChangesAsync();
      return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isCompleted = null)
    {
      if (page <= 0 || pageSize <= 0)
      {
        return BadRequest("Page and PageSize must be greater than 0.");
      }

      var query = _context.Tasks.AsQueryable();

      if (isCompleted.HasValue)
      {
        query = query.Where(t => t.IsCompleted == isCompleted.Value);
      }

      var totalItems = await query.CountAsync();

      var tasks = await query
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      return Ok(new
      {
        totalItems,
        page,
        pageSize,
        items = tasks
      });
    }

  }
}
