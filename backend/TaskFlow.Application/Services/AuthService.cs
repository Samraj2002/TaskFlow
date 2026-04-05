using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Application.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
            return ApiResponse<AuthResponseDto>.Fail("Email already registered.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(await BuildTokenResponse(user), "Registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");

        return ApiResponse<AuthResponseDto>.Ok(await BuildTokenResponse(user));
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user is null)
            return ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token.");

        return ApiResponse<AuthResponseDto>.Ok(await BuildTokenResponse(user));
    }

    public async Task<ApiResponse> LogoutAsync(Guid userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await db.SaveChangesAsync();
        }
        return ApiResponse.OkEmpty("Logged out.");
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildTokenResponse(User user)
    {
        var accessToken  = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiry       = DateTime.UtcNow.AddMinutes(GetJwtMinutes());

        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();

        return new AuthResponseDto(accessToken, refreshToken, user.Name,
                                   user.Email, user.Role.ToString(), expiry);
    }

    private string GenerateAccessToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Name),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(GetJwtMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private int GetJwtMinutes() =>
        int.TryParse(config["Jwt:ExpiryMinutes"], out var m) ? m : 60;
}
