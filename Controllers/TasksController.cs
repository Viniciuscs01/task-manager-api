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

    public TasksController(ApplicationDbContext context)
    {
      _context = context;
    }

    // Endpoint para criar uma nova tarefa
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] Models.Task task)
    {
      if (!_context.Users.Any(u => u.Id == task.UserId))
      {
        return BadRequest("Usuário inválido.");
      }

      _context.Tasks.Add(task);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    // Endpoint para obter uma tarefa por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task == null)
      {
        return NotFound();
      }

      return Ok(task);
    }

    // Endpoint para atualizar uma tarefa
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

    // Endpoint para deletar uma tarefa
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
      // Valida os parâmetros de paginação
      if (page <= 0 || pageSize <= 0)
      {
        return BadRequest("Page and PageSize must be greater than 0.");
      }

      // Consulta inicial
      var query = _context.Tasks.AsQueryable();

      // Filtro por status (isCompleted)
      if (isCompleted.HasValue)
      {
        query = query.Where(t => t.IsCompleted == isCompleted.Value);
      }

      // Total de itens antes da paginação
      var totalItems = await query.CountAsync();

      // Aplica paginação
      var tasks = await query
          .Skip((page - 1) * pageSize) // Pula itens baseados na página
          .Take(pageSize) // Limita ao tamanho da página
          .ToListAsync();

      // Retorna a resposta formatada
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
