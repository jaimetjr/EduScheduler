namespace EduScheduler.Api.Models.DTOs;

public record StudentEventDto(
    int Id,
    string Subject,
    string? BodyPreview,
    DateTime? Start,
    DateTime? End,
    string? Location,
    bool IsAllDay,
    string? OrganizerName
);
