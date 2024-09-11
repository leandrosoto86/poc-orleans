using GrainsInterfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.CodeAnalysis;
using Orleans;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PositionManagerMock.Grains
{
    //[StatelessWorker(1)]
    internal class SymbolGrain : Grain, ISymbolGrain, IObservable<Symbol>
    {
        private readonly Symbol _symbol;
        private readonly IStreamProvider _streamProvider;
        private IAsyncStream<Symbol> _streamLastQuote;

        public SymbolGrain()
        {
            _symbol = new Symbol();
            _streamProvider = this.GetStreamProvider(GlobalVariables.LiveStockStreamProvider);
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _symbol.Ticker = this.GetPrimaryKeyString();
            _streamLastQuote = _streamProvider.GetStream<Symbol>(StreamId.Create("LiveLastQuote", _symbol.Ticker));

            return Task.CompletedTask;
        }

        public ValueTask<decimal> GetLastQuote()
        {
            return ValueTask.FromResult(_symbol.LastQuote);
        }

        public async ValueTask SetLastQuote(decimal lastQuote)
        {
            _symbol.LastQuote = lastQuote;
            await _streamLastQuote.OnNextAsync(_symbol);
        }

        public ValueTask<Symbol> GetSymbol()
        {
            return ValueTask.FromResult(_symbol);
        }

        public IDisposable Subscribe(IObserver<Symbol> observer)
        {
            //Console.WriteLine($"Symbol {_symbol.Ticker} PnL subscribed");
            throw new NotImplementedException();
        }
    }
}
