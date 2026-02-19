namespace EduScheduler.Api.Models.Entities;

public class StudentEvent
{
    public int Id { get; set; }

    /// <summary>Microsoft Graph event ID</summary>
    public string GraphEventId { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public string? BodyPreview { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public string? Location { get; set; }
    public bool IsAllDay { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }

    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
}
