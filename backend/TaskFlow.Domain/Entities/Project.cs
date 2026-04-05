namespace TaskFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Creator { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
}

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
