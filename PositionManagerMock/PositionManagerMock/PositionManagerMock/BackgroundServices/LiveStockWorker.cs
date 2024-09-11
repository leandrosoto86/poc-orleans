using GrainsInterfaces;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Orleans.BroadcastChannel;
using Orleans.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PositionManagerMock.BackgroundServices
{
    internal class LiveStockWorker : BackgroundService
    {
        private readonly IStreamProvider _streamProvider;
        private readonly IGrainFactory _grainFactory;
        public LiveStockWorker(IClusterClient clusterClient, IGrainFactory grainFactory)
        {
            _streamProvider = clusterClient.GetStreamProvider(GlobalVariables.LiveStockStreamProvider); 
            _grainFactory = grainFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var instrumentList = new List<string>() { "PETR4", "VALE5", "ITUB4", "WIN", "WDL", "BOVA11", "BBDC4", "B3SA3", "ABEV3", "OIBR3" };
            var randomNumber = new Random();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    foreach (var ticker in instrumentList)
            //    {
            //        var symbolGrain = _grainFactory.GetGrain<ISymbolGrain>(ticker);
            //        var symbol = await symbolGrain.GetSymbol();

            //        var _stream = _streamProvider.GetStream<Symbol>(StreamId.Create("LiveLastQuote", symbol.Ticker));

            //        symbol.LastQuote = randomNumber.Next(1, 50);
            //        await symbolGrain.SetLastQuote(symbol.LastQuote);

            //        await _stream.OnNextAsync(symbol);

            //    }
            //    await Task.Delay(100);
            //}
        }
    }
}