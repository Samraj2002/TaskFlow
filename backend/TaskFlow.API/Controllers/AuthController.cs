using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await authService.RefreshTokenAsync(dto.RefreshToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await authService.LogoutAsync(userId);
        return Ok(result);
    }
}
