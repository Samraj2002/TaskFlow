using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Domain.Enums.TaskStatus Status { get; set; } = Domain.Enums.TaskStatus.Todo;
    public DateTime? DueDate { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Position { get; set; } // for kanban ordering

    // Navigation
    public Project Project { get; set; } = null!;
    public User? Assignee { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
}

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TaskItem Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
