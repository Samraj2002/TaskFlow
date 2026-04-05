using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Application.Services;

public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync(Guid userId)
    {
        // Projects accessible by user
        var projectIds = await db.Projects
            .Where(p => p.CreatedBy == userId || p.Members.Any(m => m.UserId == userId))
            .Select(p => p.Id)
            .ToListAsync();

        var tasks = await db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Where(t => projectIds.Contains(t.ProjectId))
            .ToListAsync();

        var now = DateTime.UtcNow;
        var total      = tasks.Count;
        var completed  = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done);
        var inProgress = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress);
        var overdue    = tasks.Count(t => t.DueDate < now && t.Status != Domain.Enums.TaskStatus.Done);

        var projects = await db.Projects
            .Where(p => projectIds.Contains(p.Id))
            .Include(p => p.Tasks)
            .Select(p => new ProjectSummaryDto(
                p.Id, p.Title,
                p.Tasks.Count,
                p.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)
            ))
            .ToListAsync();

        var myTasks = tasks
            .Where(t => t.AssigneeId == userId && t.Status != Domain.Enums.TaskStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(10)
            .Select(t => MapTask(t))
            .ToList();

        var byPriority = tasks
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityCountDto(g.Key, g.Count()))
            .ToList();

        var dashboard = new DashboardDto(
            total, completed, inProgress, overdue,
            total == 0 ? 0 : Math.Round((double)completed / total * 100, 1),
            projects, myTasks, byPriority
        );

        return ApiResponse<DashboardDto>.Ok(dashboard);
    }

    private static TaskDto MapTask(Domain.Entities.TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Priority.ToString(), t.Status.ToString(),
        t.DueDate,
        t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow,
        t.ProjectId, t.Project?.Title ?? "",
        t.AssigneeId, t.Assignee?.Name, null,
        t.Position, t.Comments.Count, t.CreatedAt
    );
}
