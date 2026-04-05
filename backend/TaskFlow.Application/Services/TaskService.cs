using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Application.Services;

public class TaskService(AppDbContext db) : ITaskService
{
    public async Task<ApiResponse<List<TaskDto>>> GetByProjectAsync(
        Guid projectId, Guid userId, string? status, string? priority, Guid? assigneeId)
    {
        if (!await HasProjectAccess(projectId, userId))
            return ApiResponse<List<TaskDto>>.Fail("Access denied.");

        var query = db.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status.ToString() == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority.ToString() == priority);

        if (assigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == assigneeId);

        var tasks = await query.OrderBy(t => t.Position).Select(t => MapToDto(t)).ToListAsync();
        return ApiResponse<List<TaskDto>>.Ok(tasks);
    }

    public async Task<ApiResponse<TaskDto>> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await GetTaskWithAccess(id, userId);
        if (task is null) return ApiResponse<TaskDto>.Fail("Task not found.");
        return ApiResponse<TaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse<TaskDto>> CreateAsync(Guid projectId, CreateTaskDto dto, Guid userId)
    {
        if (!await HasProjectAccess(projectId, userId))
            return ApiResponse<TaskDto>.Fail("Access denied.");

        // Max position in the Todo column
        var maxPos = await db.Tasks
            .Where(t => t.ProjectId == projectId && t.Status == Domain.Enums.TaskStatus.Todo)
            .MaxAsync(t => (int?)t.Position) ?? -1;

        var task = new TaskItem
        {
            Title       = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Priority    = dto.Priority,
            DueDate     = dto.DueDate,
            ProjectId   = projectId,
            AssigneeId  = dto.AssigneeId,
            Position    = maxPos + 1
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        task = await db.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .FirstAsync(t => t.Id == task.Id);

        return ApiResponse<TaskDto>.Ok(MapToDto(task), "Task created.");
    }

    public async Task<ApiResponse<TaskDto>> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId)
    {
        var task = await GetTaskWithAccess(id, userId);
        if (task is null) return ApiResponse<TaskDto>.Fail("Task not found.");

        task.Title       = dto.Title.Trim();
        task.Description = dto.Description.Trim();
        task.Priority    = dto.Priority;
        task.Status      = dto.Status;
        task.DueDate     = dto.DueDate;
        task.AssigneeId  = dto.AssigneeId;

        await db.SaveChangesAsync();
        return ApiResponse<TaskDto>.Ok(MapToDto(task), "Task updated.");
    }

    public async Task<ApiResponse<TaskDto>> MoveAsync(Guid id, MoveTaskDto dto, Guid userId)
    {
        var task = await GetTaskWithAccess(id, userId);
        if (task is null) return ApiResponse<TaskDto>.Fail("Task not found.");

        task.Status   = dto.NewStatus;
        task.Position = dto.NewPosition;

        await db.SaveChangesAsync();
        return ApiResponse<TaskDto>.Ok(MapToDto(task));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid userId)
    {
        var task = await GetTaskWithAccess(id, userId);
        if (task is null) return ApiResponse.Fail("Task not found.");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return ApiResponse.OkEmpty("Task deleted.");
    }

    public async Task<ApiResponse<CommentDto>> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid userId)
    {
        var task = await GetTaskWithAccess(taskId, userId);
        if (task is null) return ApiResponse<CommentDto>.Fail("Task not found.");

        var user = await db.Users.FindAsync(userId);
        var comment = new Comment { TaskId = taskId, UserId = userId, Content = dto.Content.Trim() };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return ApiResponse<CommentDto>.Ok(new CommentDto(
            comment.Id, comment.Content,
            user!.Name, GetInitials(user.Name),
            comment.CreatedAt));
    }

    public async Task<ApiResponse<List<CommentDto>>> GetCommentsAsync(Guid taskId, Guid userId)
    {
        var task = await GetTaskWithAccess(taskId, userId);
        if (task is null) return ApiResponse<List<CommentDto>>.Fail("Task not found.");

        var comments = await db.Comments
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.Content, c.User.Name,
                         GetInitials(c.User.Name), c.CreatedAt))
            .ToListAsync();

        return ApiResponse<List<CommentDto>>.Ok(comments);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<bool> HasProjectAccess(Guid projectId, Guid userId) =>
        await db.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId) ||
        await db.Projects.AnyAsync(p => p.Id == projectId && p.CreatedBy == userId);

    private async Task<TaskItem?> GetTaskWithAccess(Guid id, Guid userId) =>
        await db.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id &&
                (t.Project.CreatedBy == userId ||
                 t.Project.Members.Any(m => m.UserId == userId)));

    private static TaskDto MapToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Priority.ToString(), t.Status.ToString(),
        t.DueDate,
        t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Done,
        t.ProjectId, t.Project?.Title ?? "",
        t.AssigneeId, t.Assignee?.Name, t.Assignee is not null ? GetInitials(t.Assignee.Name) : null,
        t.Position, t.Comments.Count, t.CreatedAt
    );

    private static string GetInitials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Take(2).Select(w => char.ToUpper(w[0])));
}
