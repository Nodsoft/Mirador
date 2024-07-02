using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nodsoft.Mirador.Functions.Data;
using Nodsoft.Mirador.Functions.Infrastructure;
using Nodsoft.Mirador.Functions.Services;

namespace Nodsoft.Mirador.Functions;

public sealed class PingTrigger : IDisposable
{
    private readonly ILogger<PingTrigger> _logger;
    private readonly CosmosClient _dbClient;
    private readonly Container _watchersContainer;
    private readonly HttpPingService _pingService;
    private readonly DiscordPingReportService _reportService;

    public PingTrigger(ILogger<PingTrigger> logger, CosmosClient dbClient, HttpPingService pingService, DiscordPingReportService reportService)
    {
        _logger = logger;
        _dbClient = dbClient;
        _watchersContainer = _dbClient.GetDatabase("mirador").GetContainer("watchers");
        _pingService = pingService;
        _reportService = reportService;
    }

    [Function("PingTrigger")]
    public async Task RunAllAsync(
#if DEBUG
        [TimerTrigger("0 * * * * *", RunOnStartup = true)]
#else
        [TimerTrigger("0 */15 * * * *")]
#endif        
        TimerInfo myTimer,
        CancellationToken ct = default
    ) {
        FeedResponse<Watcher> watchers = await _watchersContainer.GetItemQueryIterator<Watcher>().ReadNextAsync(ct);

        await Task.WhenAll(watchers.Select(w => Task.Run(async () =>
        {
            PingAggregateResult pingResult = await _pingService.PingUrisAsync(w.PingConfigs, ct);
            w.LastCheck = pingResult.Timestamp;

            if (w.LastStatus != pingResult.Status)
            {
                w.LastStatus = pingResult.Status;
                w.LastChange = pingResult.Timestamp;
            }
            
            _logger.LogInformation("Watcher {Id} reports {Status} at {Timestamp}", w.Id, pingResult.Status, pingResult.Timestamp);
            await _reportService.ReportPingResultAsync(w.Name, w.ReportConfigs, pingResult, ct);
            await UpdateWatcher(w);
        }, ct)));
    }

    public void Dispose()
    {
        _dbClient.Dispose();
        _pingService.Dispose();
    }
    
    private async Task UpdateWatcher(Watcher watcher)
    {
        await _watchersContainer.UpsertItemAsync(watcher);
    }
}