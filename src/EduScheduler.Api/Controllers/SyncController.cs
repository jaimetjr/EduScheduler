using EduScheduler.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduScheduler.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly IGraphSyncService _syncService;

    public SyncController(IGraphSyncService syncService)
    {
        _syncService = syncService;
    }

    /// <summary>Manually trigger a full sync from Microsoft Graph</summary>
    [HttpPost("trigger")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult TriggerSync()
    {
        _ = Task.Run(async () =>
        {
            await _syncService.SyncUsersAsync();
            await _syncService.SyncAllEventsAsync();
        });

        return Accepted(new { message = "Sync started." });
    }
}
