using Microsoft.EntityFrameworkCore;

namespace TaskManager.Models
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Task>()
          .HasOne(t => t.User)
          .WithMany(u => u.Tasks)
          .HasForeignKey(t => t.UserId)
          .IsRequired(false);
    }

  }
}
