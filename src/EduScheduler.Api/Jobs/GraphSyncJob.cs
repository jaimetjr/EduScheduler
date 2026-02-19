namespace EduScheduler.Api.Jobs;

public class GraphSyncJob
{
    private readonly Services.IGraphSyncService _syncService;
    private readonly ILogger<GraphSyncJob> _logger;

    public GraphSyncJob(Services.IGraphSyncService syncService, ILogger<GraphSyncJob> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Graph sync job started at {Time}.", DateTime.UtcNow);

        await _syncService.SyncUsersAsync();
        await _syncService.SyncAllEventsAsync();

        _logger.LogInformation("Graph sync job finished at {Time}.", DateTime.UtcNow);
    }
}
