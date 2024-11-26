using System.Collections.Concurrent;

namespace TaskManager.Services
{
  public class LoginAttemptService
  {
    private readonly ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)> _attempts = new();
    private const int MaxAttempts = 5;
    private const int DelaySeconds = 2;

    public bool IsBlocked(string username)
    {
      if (_attempts.TryGetValue(username, out var data))
      {
        var (attempts, lastAttempt) = data;
        if (attempts >= MaxAttempts)
        {
          var timeSinceLastAttempt = (DateTime.UtcNow - lastAttempt).TotalSeconds;
          var blockDuration = DelaySeconds * (attempts - MaxAttempts + 1);

          if (timeSinceLastAttempt < blockDuration)
          {
            return true;
          }
        }
      }
      return false;
    }

    public void RecordFailedAttempt(string username)
    {
      _attempts.AddOrUpdate(username,
          _ => (1, DateTime.UtcNow),
          (_, data) => (data.Attempts + 1, DateTime.UtcNow));
    }

    public void ResetAttempts(string username)
    {
      _attempts.TryRemove(username, out _);
    }
  }
}