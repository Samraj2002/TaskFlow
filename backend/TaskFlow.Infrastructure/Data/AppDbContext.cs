using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ProjectMember composite key
        mb.Entity<ProjectMember>()
            .HasKey(pm => new { pm.ProjectId, pm.UserId });

        mb.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId);

        mb.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId);

        // Project → Creator (no cascade to avoid cycles)
        mb.Entity<Project>()
            .HasOne(p => p.Creator)
            .WithMany(u => u.CreatedProjects)
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskItem → Assignee (no cascade)
        mb.Entity<TaskItem>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        // TaskItem → Project (cascade)
        mb.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment → Task (cascade)
        mb.Entity<Comment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment → User (no cascade)
        mb.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Enum conversions (stored as string for readability)
        mb.Entity<User>().Property(u => u.Role).HasConversion<string>();
        mb.Entity<TaskItem>().Property(t => t.Priority).HasConversion<string>();
        mb.Entity<TaskItem>().Property(t => t.Status).HasConversion<string>();
    }
}
