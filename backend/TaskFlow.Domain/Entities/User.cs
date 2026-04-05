using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Project> CreatedProjects { get; set; } = [];
}
