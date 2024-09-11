// See https://aka.ms/new-console-template for more information


using GrainsInterfaces;
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
    })
    .Build();
try
{
    await host.StartAsync();
}
catch(Exception ex)
{
    Console.WriteLine( ex.Message);
    Console.WriteLine("Failed to connecto to cluster");
    Console.ReadKey();
    return 0;
}

var client = host.Services.GetRequiredService<IClusterClient>();

Console.Clear();
Console.WriteLine("Market data.");

var instrumentList = new List<string>() { "PETR4", "VALE5", "ITUB4", "WIN", "WDL", "BOVA11", "BBDC4", "B3SA3", "ABEV3", "OIBR3" };
var randomNumber = new Random();

while (true)
{
    foreach (var ticker in instrumentList)
    {
        try
        {
            var symbolGrain = client.GetGrain<ISymbolGrain>(ticker);
            await symbolGrain.SetLastQuote(randomNumber.Next(1, 50));
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    await Task.Delay(1000);
}

async Task<bool> RetryFilter(Exception exception, CancellationToken cancellationToken)
{
    attempt++;
    Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
    if (attempt > maxAttempts)
    {
        return false;
    }
    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    return true;
}