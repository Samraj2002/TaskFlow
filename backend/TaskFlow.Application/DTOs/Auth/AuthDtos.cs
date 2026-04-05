using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.DTOs.Auth;

public record RegisterDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    string Name,
    string Email,
    string Role,
    DateTime ExpiresAt
);

public record RefreshTokenDto(
    [Required] string RefreshToken
);
