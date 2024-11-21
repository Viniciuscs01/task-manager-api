namespace TaskManager.Services
{
  public interface IAuditLogService
  {
    Task LogAsync(string action, string username, string details);
  }
}
