namespace EduScheduler.Api.Models.DTOs;

public record StudentDto(
    int Id,
    string DisplayName,
    string Email,
    string? Department
);
