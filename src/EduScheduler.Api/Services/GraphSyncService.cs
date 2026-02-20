using EduScheduler.Api.Data;
using EduScheduler.Api.Models.Entities;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.EntityFrameworkCore;

namespace EduScheduler.Api.Services;

public class GraphSyncService : IGraphSyncService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GraphSyncService> _logger;

    public GraphSyncService(
        GraphServiceClient graphClient,
        IServiceScopeFactory scopeFactory,
        ILogger<GraphSyncService> logger)
    {
        _graphClient = graphClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SyncUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting user sync from Microsoft Graph...");

        try
        {
            var graphUsers = new List<User>();
            var usersResponse = await _graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Select = new[]
                {
                    "id", "displayName", "mail", "userPrincipalName", "department", "jobTitle"
                };
                config.QueryParameters.Top = 999;
            }, cancellationToken);

            if (usersResponse?.Value == null)
            {
                _logger.LogWarning("No users returned from Microsoft Graph.");
                return;
            }

            var pageIterator = PageIterator<User, UserCollectionResponse>
                .CreatePageIterator(
                    _graphClient,
                    usersResponse,
                    user =>
                    {
                        graphUsers.Add(user);
                        return true;
                    });

            await pageIterator.IterateAsync(cancellationToken);

            _logger.LogInformation("Fetched {Count} users from Graph.", graphUsers.Count);

            var uniqueUsers = graphUsers
                .Where(u => !string.IsNullOrEmpty(u.Id))
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Processing {Count} unique users.", uniqueUsers.Count);

            const int batchSize = 500;
            for (int i = 0; i < uniqueUsers.Count; i += batchSize)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var batch = uniqueUsers.Skip(i).Take(batchSize).ToList();
                var batchGraphIds = batch.Select(u => u.Id!).ToList();

                var existingStudents = await context.Students
                    .Where(s => batchGraphIds.Contains(s.GraphId))
                    .ToDictionaryAsync(s => s.GraphId, cancellationToken);

                foreach (var graphUser in batch)
                {
                    var email = graphUser.Mail ?? graphUser.UserPrincipalName ?? string.Empty;

                    if (existingStudents.TryGetValue(graphUser.Id!, out var existing))
                    {
                        existing.DisplayName = graphUser.DisplayName ?? existing.DisplayName;
                        existing.Email = email;
                        existing.Department = graphUser.Department;
                        existing.JobTitle = graphUser.JobTitle;
                        existing.SyncedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        context.Students.Add(new Student
                        {
                            GraphId = graphUser.Id!,
                            DisplayName = graphUser.DisplayName ?? "Unknown",
                            Email = email,
                            Department = graphUser.Department,
                            JobTitle = graphUser.JobTitle,
                            SyncedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Processed batch {Batch}/{Total}.", i + batch.Count, uniqueUsers.Count);
            }

            _logger.LogInformation("User sync completed. {Count} users processed.", uniqueUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing users from Microsoft Graph.");
            throw;
        }
    }

    public async Task SyncUserEventsAsync(string graphUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventsResponse = await _graphClient.Users[graphUserId].Events.GetAsync(config =>
            {
                config.QueryParameters.Select = new[]
                {
                    "id", "subject", "bodyPreview", "start", "end",
                    "location", "isAllDay", "organizer"
                };
                config.QueryParameters.Top = 250;
                config.QueryParameters.Orderby = new[] { "start/dateTime desc" };
            }, cancellationToken);

            if (eventsResponse?.Value == null) return;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var student = await context.Students
                .FirstOrDefaultAsync(s => s.GraphId == graphUserId, cancellationToken);

            if (student == null) return;

            foreach (var graphEvent in eventsResponse.Value)
            {
                if (string.IsNullOrEmpty(graphEvent.Id)) continue;

                var existing = await context.StudentEvents
                    .FirstOrDefaultAsync(e =>
                        e.GraphEventId == graphEvent.Id && e.StudentId == student.Id,
                        cancellationToken);

                var startDate = graphEvent.Start?.DateTime != null
                    ? DateTime.Parse(graphEvent.Start.DateTime)
                    : (DateTime?)null;

                var endDate = graphEvent.End?.DateTime != null
                    ? DateTime.Parse(graphEvent.End.DateTime)
                    : (DateTime?)null;

                if (existing != null)
                {
                    existing.Subject = graphEvent.Subject ?? existing.Subject;
                    existing.BodyPreview = graphEvent.BodyPreview;
                    existing.Start = startDate;
                    existing.End = endDate;
                    existing.Location = graphEvent.Location?.DisplayName;
                    existing.IsAllDay = graphEvent.IsAllDay ?? false;
                    existing.OrganizerName = graphEvent.Organizer?.EmailAddress?.Name;
                    existing.OrganizerEmail = graphEvent.Organizer?.EmailAddress?.Address;
                    existing.SyncedAt = DateTime.UtcNow;
                }
                else
                {
                    context.StudentEvents.Add(new StudentEvent
                    {
                        GraphEventId = graphEvent.Id,
                        Subject = graphEvent.Subject ?? "No Subject",
                        BodyPreview = graphEvent.BodyPreview,
                        Start = startDate,
                        End = endDate,
                        Location = graphEvent.Location?.DisplayName,
                        IsAllDay = graphEvent.IsAllDay ?? false,
                        OrganizerName = graphEvent.Organizer?.EmailAddress?.Name,
                        OrganizerEmail = graphEvent.Organizer?.EmailAddress?.Address,
                        StudentId = student.Id,
                        SyncedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing events for user {UserId}.", graphUserId);
        }
    }

    public async Task SyncAllEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full event sync...");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var graphIds = await context.Students
            .Select(s => s.GraphId)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Syncing events for {Count} students.", graphIds.Count);

        foreach (var graphId in graphIds)
        {
            await SyncUserEventsAsync(graphId, cancellationToken);
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation("Full event sync completed.");
    }
}
