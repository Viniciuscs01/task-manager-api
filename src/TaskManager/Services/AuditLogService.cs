using TaskManager.Models;

namespace TaskManager.Services
{
  public class AuditLogService : IAuditLogService
  {
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async System.Threading.Tasks.Task LogAsync(string action, string username, string details)
    {
      var auditLog = new AuditLog
      {
        Action = action,
        Username = username,
        Timestamp = DateTime.UtcNow,
        Details = details
      };

      _context.AuditLogs.Add(auditLog);
      await _context.SaveChangesAsync();
    }
  }
}
