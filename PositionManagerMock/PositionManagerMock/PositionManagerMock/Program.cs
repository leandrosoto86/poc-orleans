// Configure the host
using GrainsInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Development;
using PositionManagerMock;
using PositionManagerMock.BackgroundServices;
using PositionManagerMock.Grains;
using System.Diagnostics.Metrics;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new HostBuilder()
           .UseOrleans(builder => {
               builder.UseLocalhostClustering();
               //builder.AddDynamoDBGrainStorage(
               //     name: "custodyStore",
               //     dynamoConfig =>
               //     {
               //         dynamoConfig.TableName = "orleans-state";
               //         dynamoConfig.Service = "us-east-2";
               //         dynamoConfig.AccessKey = "xxx";
               //         dynamoConfig.SecretKey = "xxx";
               //         dynamoConfig.CreateIfNotExists = true;
               //     }
               // );
               builder.Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = "position-manager-cluster";
                   options.ServiceId = "position-manager-service";
               });
               builder.ConfigureLogging(logging => logging.AddConsole());
               builder.UseDashboard();
               builder.AddMemoryStreams(GlobalVariables.LiveStockStreamProvider);
               builder.AddMemoryStreams(GlobalVariables.LivePnLStreamProvider);
               builder.AddMemoryGrainStorage(GlobalVariables.PubSubStore);
               builder.AddBroadcastChannel(GlobalVariables.LiveStockBroadcastChannel);
           })
           //.ConfigureServices(
           //    services => services.AddHostedService<LiveStockWorker>())
           .Build();

        // Start the host
        await host.StartAsync();

        Console.WriteLine("Orleans is running.\nPress Enter to terminate...");
        Console.ReadLine();
        Console.WriteLine("Orleans is stopping...");

        await host.StopAsync();
    }
}