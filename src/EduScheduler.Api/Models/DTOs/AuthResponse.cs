namespace EduScheduler.Api.Models.DTOs;

public record AuthResponse(
    string Token,
    string Username,
    DateTime ExpiresAt
);
