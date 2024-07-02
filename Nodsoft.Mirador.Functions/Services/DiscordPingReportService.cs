using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Nodsoft.Mirador.Functions.Data;
using Nodsoft.Mirador.Functions.Infrastructure;

namespace Nodsoft.Mirador.Functions.Services;

/// <summary>
/// Provides a service for reporting ping results to Discord.
/// </summary>
public sealed class DiscordPingReportService
{
    private readonly ILogger<DiscordPingReportService> _logger;

    public DiscordPingReportService(ILogger<DiscordPingReportService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Reports the specified ping result to a Discord webhook.
    /// </summary>
    /// <param name="name">The display name of the watcher.</param>
    /// <param name="reportConfig">The report configuration for the watcher.</param>
    /// <param name="pingResult">The ping result to report.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task ReportPingResultAsync(string name, WatcherReportConfig reportConfig, PingAggregateResult pingResult, CancellationToken ct = default)
    {
        using DiscordWebhookClient webhookClient = new(reportConfig.WebhookUri.AbsoluteUri);

        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithTitle(name)
            .WithDescription(pingResult.Status switch
            {
                HealthStatus.Healthy => reportConfig.HealthyMessage,
                HealthStatus.Degraded => reportConfig.DegradedMessage,
                HealthStatus.Unhealthy => reportConfig.UnhealthyMessage,
                _ => reportConfig.UnknownMessage
            })
            .WithTimestamp(pingResult.Timestamp)
            .WithColor(pingResult.Status switch
            {
                HealthStatus.Healthy => Color.Green,
                HealthStatus.Degraded => Color.Orange,
                HealthStatus.Unhealthy => Color.Red,
                _ => Color.Default
            })
            .WithAuthor(reportConfig.Username, iconUrl: reportConfig.AvatarUrl?.AbsoluteUri);

        foreach (PingResult result in pingResult.Results)
        {
            embedBuilder.AddField(
                result.EndpointUri.DnsSafeHost,
                result.Status.ToString(),
                inline: true
            );
        }

        if (reportConfig.MessageId is null or "")
        {
            ulong mid = await webhookClient.SendMessageAsync(embeds: new[] { embedBuilder.Build() });
            reportConfig.MessageId = mid.ToString();
        }
        else
        {
            await webhookClient.ModifyMessageAsync(ulong.Parse(reportConfig.MessageId), p => p.Embeds = new[] { embedBuilder.Build() });
        }
    }
    
    /// <summary>
    /// Reports the specified ping result to several Discord webhooks.
    /// </summary>
    /// <param name="name">The display name of the watcher.</param>
    /// <param name="reportConfigs">The report configurations for the watcher.</param>
    /// <param name="pingResult">The ping result to report.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task ReportPingResultAsync(string name, IEnumerable<WatcherReportConfig> reportConfigs, PingAggregateResult pingResult, CancellationToken ct = default)
    {
        await Task.WhenAll(reportConfigs.Select(async rc =>
        {
            try
            {
                await ReportPingResultAsync(name, rc, pingResult, ct);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to report ping result for {Name} to {WebhookUri}", name, rc.WebhookUri);
                throw;
            }
        }));
    }
}