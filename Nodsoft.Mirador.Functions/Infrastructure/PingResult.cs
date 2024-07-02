using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nodsoft.Mirador.Functions.Infrastructure;

public sealed class PingResult
{
    public HealthStatus Status { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Uri EndpointUri { get; set; }
    
    public Exception? Exception { get; set; }
    
    public PingResult(HealthStatus status, DateTimeOffset timestamp, Uri endpointUri, Exception? exception = null)
    {
        Status = status;
        Timestamp = timestamp;
        EndpointUri = endpointUri;
        Exception = exception;
    }
}