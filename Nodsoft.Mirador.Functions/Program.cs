using System.Reflection;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nodsoft.Mirador.Functions;
using Nodsoft.Mirador.Functions.Services;
using Throw;

string? miradorVersion = typeof(PingTrigger).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+').First();
TokenCredential azCred = new DefaultAzureCredential();

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((host, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton<CosmosClient>(s => new(
#if DEBUG
            s.GetRequiredService<IConfiguration>().GetConnectionString("cosmos").ThrowIfNull(),
#else
            accountEndpoint: Environment.GetEnvironmentVariable("AZURE_COSMOS_RESOURCEENDPOINT")!,
            tokenCredential: azCred,
#endif            
            new()
            {
                SerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
#if DEBUG
                ServerCertificateCustomValidationCallback = (_, _, _) => true
#endif
            })
        );

        services.AddSingleton<DiscordPingReportService>();
        
        services.AddScoped<HttpPingService>();
        services.AddHttpClient<HttpPingService>()
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"NSYS-Mirador/{miradorVersion} (+https://github.com/Nodsoft/Mirador; mailto:admin+mirador@nodsoft.net)");
            })
            .AddStandardResilienceHandler();

    })
    .Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
using CosmosClient dbClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
Database db = await dbClient.CreateDatabaseIfNotExistsAsync("mirador");
await db.CreateContainerIfNotExistsAsync("watchers", "/id");

await host.RunAsync();