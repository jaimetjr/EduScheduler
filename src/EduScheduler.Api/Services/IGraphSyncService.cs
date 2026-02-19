namespace EduScheduler.Api.Services;

public interface IGraphSyncService
{
    Task SyncUsersAsync(CancellationToken cancellationToken = default);
    Task SyncUserEventsAsync(string graphUserId, CancellationToken cancellationToken = default);
    Task SyncAllEventsAsync(CancellationToken cancellationToken = default);
}
