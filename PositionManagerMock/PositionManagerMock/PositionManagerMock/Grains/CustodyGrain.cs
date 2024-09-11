using GrainsInterfaces;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Orleans.Streams.Core;
using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Xml;

namespace PositionManagerMock.Grains
{

    public class CustodyGrain : Grain, ICustodyGrain, IAsyncObserver<Symbol>, IAsyncObservable<decimal>
    {
        private readonly Orleans.Streams.IStreamProvider _streamProviderLiveQuote;
        private readonly Orleans.Streams.IStreamProvider _streamProviderPnL;
        private readonly IGrainFactory _grainFactory;

        private IAsyncStream<Symbol> _streamLiveQuote;
        private IAsyncStream<decimal> _streamPnl;

        //private readonly IPersistentState<Custody> _custodyState; Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
        private readonly Custody _custodyState;

        public CustodyGrain(IGrainFactory grainFactory
            //[PersistentState("custody", "custodyStore")] IPersistentState<Custody> custodyState Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
            )
        {
            _grainFactory = grainFactory;
            _streamProviderLiveQuote = this.GetStreamProvider(GlobalVariables.LiveStockStreamProvider);
            _streamProviderPnL = this.GetStreamProvider(GlobalVariables.LivePnLStreamProvider);
            _custodyState = new Custody();
        }

        private async ValueTask SubscribeMarketData()
        {
            //Subscribe Live Quotes
            foreach (var asset in _custodyState.Assets)
            {
                var streamId = StreamId.Create("LiveLastQuote", asset.Value.Symbol);
                _streamLiveQuote = _streamProviderLiveQuote.GetStream<Symbol>(streamId);

                var handles = await _streamLiveQuote.GetAllSubscriptionHandles();

                if (handles.Any())
                {
                    await handles[0].ResumeAsync(this);
                }
                else
                {
                    await _streamLiveQuote.SubscribeAsync<Symbol>(OnNextAsync, OnErrorAsync, OnCompletedAsync, null);
                }

            }
        }
        public async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            //await _custodyState.ReadStateAsync(); Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
            await SubscribeMarketData(); 

            //Publisher PnL
            _streamPnl = _streamProviderPnL.GetStream<decimal>(StreamId.Create("PnL", this.GetPrimaryKeyLong()));

        }
        public async override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            foreach (var asset in _custodyState.Assets)
            {
                var symbolGrain = _grainFactory.GetGrain<ISymbolGrain>(asset.Value.Symbol);
                var symbol = await symbolGrain.GetSymbol();

                var streamProvider = this.GetStreamProvider(GlobalVariables.LiveStockStreamProvider);
                _streamLiveQuote = _streamProviderLiveQuote.GetStream<Symbol>(StreamId.Create("LiveLastQuote", symbol.Ticker));

                var handles = await _streamLiveQuote.GetAllSubscriptionHandles();

                foreach (var hanle in handles)
                {
                    await hanle.UnsubscribeAsync();
                }
            }
        }

        public async ValueTask CreateRandomCustody()
        {
            if (!_custodyState.Assets.Any())
            {
                var randomNumber = new Random();

                var assetList = new List<string> { "PETR4", "VALE5", "ITUB4", "WIN", "WDL", "BOVA11", "BBDC4", "B3SA3", "ABEV3", "OIBR3" };

                for (int i = 0; i <= randomNumber.Next(1, assetList.Count - 1); i++)
                {
                    Asset? asset;
                    if (!_custodyState.Assets.TryGetValue(assetList[i], out asset))
                    {
                        asset = new Asset();
                        asset.BlockedValue = 0;
                        asset.Value = 0;
                        asset.Quantity = randomNumber.Next(1, 100);
                        asset.QuantityBlocked = randomNumber.Next(1, 100);
                        asset.Symbol = assetList[i];

                        _custodyState.Assets.Add(assetList[i], asset);
                    }
                }
                //await _custodyState.WriteStateAsync(); Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
                await SubscribeMarketData();
            }
        }

        public ValueTask<Custody> GetCustody()
        {
            return ValueTask.FromResult(_custodyState);
        }

        public ValueTask<Asset?> GetAsset(string symbol)
        {
            Asset? asset;
            _custodyState.Assets.TryGetValue(symbol, out asset);

            return ValueTask.FromResult(asset);
        }

        public ValueTask<decimal> GetPnL()
        {
            return ValueTask.FromResult(_custodyState.PnL);
        }

        public async ValueTask AddAsset(Asset asset)
        {
            Asset? existingAsset;
            if(!_custodyState.Assets.TryGetValue(asset.Symbol, out existingAsset))
            {
                _custodyState.Assets.Add(asset.Symbol, asset);
                //await _custodyState.WriteStateAsync(); Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
                await SubscribeMarketData();
            }
        }

        public async ValueTask<bool> BlockAssetIfAvailable(string symbol, decimal qtty)
        {
            Asset? asset;
            if (_custodyState.Assets.TryGetValue(symbol, out asset))
            {
                if (asset.GetAvaliableQtty() >= qtty)
                {
                    asset.QuantityBlocked = asset.QuantityBlocked + qtty;
                    //await _custodyState.WriteStateAsync(); Para manipular o estado é necessário configurar um provider no startup do silo -> https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0
                    return true;
                }
            }
            return false;
        }

        public async Task OnNextAsync(Symbol item, StreamSequenceToken? token = null)
        {
            Asset? asset;
            _custodyState.Assets.TryGetValue(item.Ticker, out asset);

            if (asset != null)
            {
                var oldAssetValue = asset.Value;
                asset.Value = asset.Quantity * item.LastQuote;
                _custodyState.PnL = _custodyState.PnL - oldAssetValue + asset.Value;
            }

            await _streamPnl.OnNextAsync(_custodyState.PnL);
        }

        public Task OnCompletedAsync()
        {
            //Console.WriteLine("OnCompletedAsync");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task<StreamSubscriptionHandle<decimal>> SubscribeAsync(IAsyncObserver<decimal> observer)
        {
            throw new NotImplementedException();
        }

        public Task<StreamSubscriptionHandle<decimal>> SubscribeAsync(IAsyncObserver<decimal> observer, StreamSequenceToken? token, string? filterData = null)
        {
            throw new NotImplementedException();
        }


    }
}
    