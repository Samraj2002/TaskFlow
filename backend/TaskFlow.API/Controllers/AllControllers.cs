using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.API.Controllers;

[ApiController, Authorize, Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await projectService.GetAllAsync(UserId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await projectService.GetByIdAsync(id, UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await projectService.CreateAsync(dto, UserId);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var result = await projectService.UpdateAsync(id, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await projectService.DeleteAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> InviteMember(Guid id, [FromBody] InviteMemberDto dto)
    {
        var result = await projectService.InviteMemberAsync(id, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        var result = await projectService.RemoveMemberAsync(id, memberId, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController, Authorize, Route("api/projects/{projectId:guid}/tasks")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid projectId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assigneeId) =>
        Ok(await taskService.GetByProjectAsync(projectId, UserId, status, priority, assigneeId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid id)
    {
        var result = await taskService.GetByIdAsync(id, UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskDto dto)
    {
        var result = await taskService.CreateAsync(projectId, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid id, [FromBody] UpdateTaskDto dto)
    {
        var result = await taskService.UpdateAsync(id, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> Move(Guid projectId, Guid id, [FromBody] MoveTaskDto dto)
    {
        var result = await taskService.MoveAsync(id, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        var result = await taskService.DeleteAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid projectId, Guid id) =>
        Ok(await taskService.GetCommentsAsync(id, UserId));

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid projectId, Guid id, [FromBody] CreateCommentDto dto)
    {
        var result = await taskService.AddCommentAsync(id, dto, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController, Authorize, Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await dashboardService.GetDashboardAsync(UserId));
}
