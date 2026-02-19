namespace EduScheduler.Api.Models.Entities;

public class Student
{
    public int Id { get; set; }

    /// <summary>Microsoft Graph user ID</summary>
    public string GraphId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }

    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StudentEvent> Events { get; set; } = new List<StudentEvent>();
}
