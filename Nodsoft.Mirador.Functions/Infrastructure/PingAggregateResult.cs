using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nodsoft.Mirador.Functions.Infrastructure;

public sealed class PingAggregateResult
{
    public HealthStatus Status { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public PingResult[] Results { get; set; } = [];
    
    public static PingAggregateResult FromResults(PingResult[] results)
    {
        PingAggregateResult aggResult = new()
        {
            Results = results,
            Status = results.Min(r => r.Status),
            Timestamp = results.Max(r => r.Timestamp)
        };
        
        return aggResult;
    }
}