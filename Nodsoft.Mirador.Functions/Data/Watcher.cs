using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nodsoft.Mirador.Functions.Data;

/// <summary>
/// Defines a periodically monitored remote HTTP resource, and associated state.
/// </summary>
public class Watcher
{
    /// <summary>
    /// The unique identifier for the watcher.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Display name for the watcher.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Defines if the watcher is disabled.
    /// </summary>
    public bool Disabled { get; set; }
    
    /// <summary>
    /// The last known status of the monitored resource.
    /// </summary>
    public HealthStatus LastStatus { get; set; }
    
    /// <summary>
    /// The last time the monitored resource was checked.
    /// </summary>
    public DateTimeOffset LastCheck { get; set; }
    
    /// <summary>
    /// The last time the monitored resource state has changed.
    /// </summary>
    /// <seealso cref="LastStatus"/>
    public DateTimeOffset LastChange { get; set; }

    /// <summary>
    /// The configurations for monitoring the resource.
    /// </summary>
    /// <remarks>
    /// Watcher will report the worst status of all ping configurations.
    /// </remarks>
    public WatcherPingConfig[] PingConfigs { get; set; } = [];

    /// <summary>
    /// The configurations for reporting the watcher's state.
    /// </summary>
    /// <remarks>
    /// Watcher will report to all report configurations.
    /// </remarks>
    public WatcherReportConfig[] ReportConfigs { get; set; } = [];
}