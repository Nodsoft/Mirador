using System.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nodsoft.Mirador.Functions.Data;
using Nodsoft.Mirador.Functions.Infrastructure;

namespace Nodsoft.Mirador.Functions.Services;

/// <summary>
/// Provides a service for pinging HTTP resources.
/// </summary>
public sealed class HttpPingService : IDisposable
{
    private readonly HttpClient _httpClient;

    public HttpPingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Pings the specified URI.
    /// </summary>
    /// <param name="pingConfig">The configuration for the ping.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task<PingResult> PingUriAsync(WatcherPingConfig pingConfig, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(pingConfig.PingEndpointUri, ct);

            return new(
                response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Degraded,
                DateTimeOffset.UtcNow,
                pingConfig.PingEndpointUri
            );
        }
        catch (Exception e)
        {
            return new(HealthStatus.Unhealthy, DateTimeOffset.UtcNow, pingConfig.PingEndpointUri, e);
        }
    }
    
    public async Task<PingAggregateResult> PingUrisAsync(IEnumerable<WatcherPingConfig> pingConfigs, CancellationToken ct = default)
    {
        PingResult[] results = await Task.WhenAll(pingConfigs.Select(p => PingUriAsync(p, ct)));
        return PingAggregateResult.FromResults(results);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}