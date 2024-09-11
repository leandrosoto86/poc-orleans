using GetDataFromClusterMock;
using GrainsInterfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;


int attempt = 0;
int maxAttempts = 5;

var host = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(clientBuilder => {
        clientBuilder.UseConnectionRetryFilter(RetryFilter);
        clientBuilder.UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "position-manager-cluster";
                options.ServiceId = "position-manager-service";
            });
        clientBuilder.AddMemoryStreams("LivePnLStreamProvider");
    })
    .Build();

try
{
    await host.StartAsync();
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Failed to connecto to cluster");
    Console.ReadKey();
    return;
}

var client = host.Services.GetRequiredService<IClusterClient>();

var orleansExamples = new Examples(client);

Console.Clear();

//await orleansExamples.GetSymbols();

//Get or create our first grain!
//await orleansExamples.FirstGrain(1);

//Get or create N Grains
await orleansExamples.NGrains(4000);

//Add asset
//await orleansExamples.AddAsset(999);

//Block
//await orleansExamples.BlockAsset(20, "OIBR3");

//Pooling a grain
await orleansExamples.PoolingAGrain(999);

//Streaming a grain
//await orleansExamples.StreamingAGrain(512);

Console.WriteLine("-------------------");
Console.WriteLine(  "Press any key to exit.");
Console.ReadLine();



async Task<bool> RetryFilter(Exception exception, CancellationToken cancellationToken)
{
    attempt++;
    Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
    if (attempt > maxAttempts)
    {
        return false;
    }
    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
    return true;
}
