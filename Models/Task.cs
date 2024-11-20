using System.Text.Json.Serialization;

namespace TaskManager.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}
