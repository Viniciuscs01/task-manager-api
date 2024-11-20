using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
    {
      _context = context;
      _logger = logger;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
      _logger.LogInformation("Fetching task with ID: {Id}", id);

      var task = await _context.Tasks.FindAsync(id);
      if (task == null)
      {
        _logger.LogWarning("Task with ID {Id} not found", id);
        return NotFound();
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
