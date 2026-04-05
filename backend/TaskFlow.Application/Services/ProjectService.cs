using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Application.Services;

public class ProjectService(AppDbContext db) : IProjectService
{
    public async Task<ApiResponse<List<ProjectDto>>> GetAllAsync(Guid userId)
    {
        var projects = await db.Projects
            .Where(p => p.CreatedBy == userId ||
                        p.Members.Any(m => m.UserId == userId))
            .Include(p => p.Creator)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return ApiResponse<List<ProjectDto>>.Ok(projects);
    }

    public async Task<ApiResponse<ProjectDto>> GetByIdAsync(Guid id, Guid userId)
    {
        var project = await FindAccessibleProject(id, userId);
        if (project is null) return ApiResponse<ProjectDto>.Fail("Project not found.");
        return ApiResponse<ProjectDto>.Ok(MapToDto(project));
    }

    public async Task<ApiResponse<ProjectDto>> CreateAsync(CreateProjectDto dto, Guid userId)
    {
        var project = new Project
        {
            Title       = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CreatedBy   = userId
        };

        // Creator is automatically a member
        project.Members.Add(new ProjectMember { ProjectId = project.Id, UserId = userId });

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Reload with navigation props
        project = await db.Projects
            .Include(p => p.Creator)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstAsync(p => p.Id == project.Id);

        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project created.");
    }

    public async Task<ApiResponse<ProjectDto>> UpdateAsync(Guid id, UpdateProjectDto dto, Guid userId)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null || project.CreatedBy != userId)
            return ApiResponse<ProjectDto>.Fail("Project not found or access denied.");

        project.Title       = dto.Title.Trim();
        project.Description = dto.Description.Trim();
        await db.SaveChangesAsync();

        project = await db.Projects
            .Include(p => p.Creator)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstAsync(p => p.Id == id);

        return ApiResponse<ProjectDto>.Ok(MapToDto(project), "Project updated.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid userId)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null || project.CreatedBy != userId)
            return ApiResponse.Fail("Project not found or access denied.");

        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return ApiResponse.OkEmpty("Project deleted.");
    }

    public async Task<ApiResponse> InviteMemberAsync(Guid projectId, InviteMemberDto dto, Guid requesterId)
    {
        var project = await db.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null || project.CreatedBy != requesterId)
            return ApiResponse.Fail("Project not found or access denied.");

        var invitee = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (invitee is null) return ApiResponse.Fail("User not found.");

        if (project.Members.Any(m => m.UserId == invitee.Id))
            return ApiResponse.Fail("User is already a member.");

        project.Members.Add(new ProjectMember { ProjectId = projectId, UserId = invitee.Id });
        await db.SaveChangesAsync();
        return ApiResponse.OkEmpty($"{invitee.Name} added to project.");
    }

    public async Task<ApiResponse> RemoveMemberAsync(Guid projectId, Guid memberId, Guid requesterId)
    {
        var project = await db.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null || project.CreatedBy != requesterId)
            return ApiResponse.Fail("Project not found or access denied.");

        if (memberId == requesterId) return ApiResponse.Fail("Cannot remove yourself as owner.");

        var member = project.Members.FirstOrDefault(m => m.UserId == memberId);
        if (member is null) return ApiResponse.Fail("Member not found.");

        project.Members.Remove(member);
        await db.SaveChangesAsync();
        return ApiResponse.OkEmpty("Member removed.");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<Project?> FindAccessibleProject(Guid id, Guid userId) =>
        await db.Projects
            .Include(p => p.Creator)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id &&
                (p.CreatedBy == userId || p.Members.Any(m => m.UserId == userId)));

    private static ProjectDto MapToDto(Project p) => new(
        p.Id, p.Title, p.Description,
        p.Creator?.Name ?? "Unknown",
        p.CreatedAt,
        p.Members.Count,
        p.Tasks.Count
    );
}
