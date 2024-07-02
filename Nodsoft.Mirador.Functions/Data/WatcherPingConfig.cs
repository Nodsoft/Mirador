namespace Nodsoft.Mirador.Functions.Data;

/// <summary>
/// Represents the monitoring configuration for a <see cref="Watcher"/>.
/// </summary>
public sealed class WatcherPingConfig
{
    /// <summary>
    /// The URI of the resource to monitor.
    /// </summary>
    public Uri PingEndpointUri { get; set; }
}