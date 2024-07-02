using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nodsoft.Mirador.Functions.Data;

/// <summary>
/// Represents the configuration for reporting a <see cref="Watcher"/>'s state.
/// </summary>
/// <remarks>
/// Watcher reports to a Discord webhook, and may include additional configuration.
/// </remarks>
public class WatcherReportConfig
{
    /// <summary>
    /// The URI of the Discord webhook to report to.
    /// </summary>
    public Uri WebhookUri { get; set; }
    
    /// <summary>
    /// The message Snowflake ID to use when updating a report message.
    /// </summary>
    public ulong MessageId { get; set; }
    
    /// <summary>
    /// The username to use when reporting to the webhook.
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// The avatar URL to use when reporting to the webhook.
    /// </summary>
    public Uri? AvatarUrl { get; set; }
    
    /// <summary>
    /// The message to send when the watcher's state changes to <see cref="HealthStatus.Healthy"/>.
    /// </summary>
    public string HealthyMessage { get; set; }
    
    /// <summary>
    /// The message to send when the watcher's state changes to <see cref="HealthStatus.Degraded"/>.
    /// </summary>
    public string DegradedMessage { get; set; }
    
    /// <summary>
    /// The message to send when the watcher's state changes to <see cref="HealthStatus.Unhealthy"/>.
    /// </summary>
    public string UnhealthyMessage { get; set; }
    
    /// <summary>
    /// The message to send when the watcher's state is unknown.
    /// </summary>
    public string UnknownMessage { get; set; }
}