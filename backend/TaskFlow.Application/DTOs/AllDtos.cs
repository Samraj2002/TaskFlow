using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Projects;

public record CreateProjectDto(
    [Required, StringLength(200)] string Title,
    string Description = ""
);

public record UpdateProjectDto(
    [Required, StringLength(200)] string Title,
    string Description = ""
);

public record ProjectDto(
    Guid Id,
    string Title,
    string Description,
    string CreatorName,
    DateTime CreatedAt,
    int MemberCount,
    int TaskCount
);

public record InviteMemberDto(
    [Required, EmailAddress] string Email
);

namespace TaskFlow.Application.DTOs.Tasks;

public record CreateTaskDto(
    [Required, StringLength(300)] string Title,
    string Description = "",
    TaskPriority Priority = TaskPriority.Medium,
    DateTime? DueDate = null,
    Guid? AssigneeId = null
);

public record UpdateTaskDto(
    [Required, StringLength(300)] string Title,
    string Description = "",
    TaskPriority Priority = TaskPriority.Medium,
    Domain.Enums.TaskStatus Status = Domain.Enums.TaskStatus.Todo,
    DateTime? DueDate = null,
    Guid? AssigneeId = null
);

public record MoveTaskDto(
    [Required] Domain.Enums.TaskStatus NewStatus,
    int NewPosition
);

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTime? DueDate,
    bool IsOverdue,
    Guid ProjectId,
    string ProjectTitle,
    Guid? AssigneeId,
    string? AssigneeName,
    string? AssigneeInitials,
    int Position,
    int CommentCount,
    DateTime CreatedAt
);

public record CreateCommentDto(
    [Required, StringLength(1000)] string Content
);

public record CommentDto(
    Guid Id,
    string Content,
    string AuthorName,
    string AuthorInitials,
    DateTime CreatedAt
);

namespace TaskFlow.Application.DTOs.Dashboard;

public record DashboardDto(
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int OverdueTasks,
    double CompletionPercent,
    IEnumerable<ProjectSummaryDto> Projects,
    IEnumerable<TaskDto> MyTasks,
    IEnumerable<PriorityCountDto> TasksByPriority
);

public record ProjectSummaryDto(Guid Id, string Title, int Total, int Completed);
public record PriorityCountDto(string Priority, int Count);
