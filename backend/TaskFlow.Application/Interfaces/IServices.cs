using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.DTOs.Dashboard;

namespace TaskFlow.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse> LogoutAsync(Guid userId);
}

public interface IProjectService
{
    Task<ApiResponse<List<ProjectDto>>> GetAllAsync(Guid userId);
    Task<ApiResponse<ProjectDto>> GetByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<ProjectDto>> CreateAsync(CreateProjectDto dto, Guid userId);
    Task<ApiResponse<ProjectDto>> UpdateAsync(Guid id, UpdateProjectDto dto, Guid userId);
    Task<ApiResponse> DeleteAsync(Guid id, Guid userId);
    Task<ApiResponse> InviteMemberAsync(Guid projectId, InviteMemberDto dto, Guid requesterId);
    Task<ApiResponse> RemoveMemberAsync(Guid projectId, Guid memberId, Guid requesterId);
}

public interface ITaskService
{
    Task<ApiResponse<List<TaskDto>>> GetByProjectAsync(Guid projectId, Guid userId, string? status, string? priority, Guid? assigneeId);
    Task<ApiResponse<TaskDto>> GetByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<TaskDto>> CreateAsync(Guid projectId, CreateTaskDto dto, Guid userId);
    Task<ApiResponse<TaskDto>> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId);
    Task<ApiResponse<TaskDto>> MoveAsync(Guid id, MoveTaskDto dto, Guid userId);
    Task<ApiResponse> DeleteAsync(Guid id, Guid userId);
    Task<ApiResponse<CommentDto>> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid userId);
    Task<ApiResponse<List<CommentDto>>> GetCommentsAsync(Guid taskId, Guid userId);
}

public interface IDashboardService
{
    Task<ApiResponse<DashboardDto>> GetDashboardAsync(Guid userId);
}
