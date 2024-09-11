using GrainsInterfaces;
using Newtonsoft.Json.Linq;
using Orleans.Streams;
using System.Diagnostics;

namespace GetDataFromClusterMock
{
    internal class Examples
    {

        private readonly IClusterClient _client;
        private readonly Stopwatch _stopwatch;

        public Examples(IClusterClient client)
        {
            _client = client;
            _stopwatch = new Stopwatch();
        }

        public async Task GetSymbols()
        {
            var instrumentList = new List<string>() { "PETR4", "VALE5", "ITUB4", "WIN", "WDL", "BOVA11", "BBDC4", "B3SA3", "ABEV3", "OIBR3" };
            while (true)
            {
                foreach (var item in instrumentList)
                {
                    try
                    {
                        var symbolGrain = _client.GetGrain<ISymbolGrain>(item);
                        var lq = await symbolGrain.GetLastQuote();
                        Console.WriteLine($"Ticker: {item} - LQ: {lq}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                await Task.Delay(10);
                Console.Clear();
            }

        }

        public async Task FirstGrain(int cci)
        {
            Console.WriteLine("-----------");
            Console.WriteLine( "[FirstGrain]");
            Console.WriteLine("-----------");
            try
            {
                _stopwatch.Restart();
                var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                await custodyGrain.CreateRandomCustody();
                _stopwatch.Stop();
                Console.WriteLine($"[FirstGrain] - CreateRandomCustody - Cluster elapsed time: {_stopwatch.ElapsedMilliseconds}ms");

                _stopwatch.Restart();
                var custody = await custodyGrain.GetCustody();
                _stopwatch.Stop();
                Console.WriteLine($"[FirstGrain] - GetCustody - Cluster elapsed time: {_stopwatch.ElapsedMilliseconds}ms");

                Console.WriteLine($"CCI: {cci}");
                Console.WriteLine($"PnL: {custody.PnL}");
                foreach (var item in custody.Assets)
                {
                    Console.WriteLine($"Symbol: {item.Value.Symbol} - Qtty: {item.Value.Quantity} - QttyBlocked: {item.Value.QuantityBlocked} - Value: {item.Value.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task NGrains(int ccis)
        {

            //Console.WriteLine("-----------");
            //Console.WriteLine("[NGrains]");
            //Console.WriteLine("-----------");
            string? again = "s";
            do
            {
                long elapsedTime = 0;
                for (int cci = 0; cci < ccis; cci++)
                {
                    _stopwatch.Restart();
                    var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                    await custodyGrain.CreateRandomCustody();
                    _stopwatch.Stop();

                    elapsedTime = elapsedTime + _stopwatch.ElapsedMilliseconds;

                    Console.WriteLine($"Random custody for CCI {cci} created");
                }
                Console.WriteLine($"[NGrains] - CreateRandomCustody elapsed time: {elapsedTime / ccis}ms");
                Console.ReadKey();

                elapsedTime = 0;
                for (int cci = 0; cci < ccis; cci++)
                {
                    try
                    {
                        _stopwatch.Restart();
                        var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                        var custody = await custodyGrain.GetCustody();
                        _stopwatch.Stop();

                        elapsedTime = elapsedTime + _stopwatch.ElapsedMilliseconds;

                        Console.WriteLine($"CCI: {cci}");
                        Console.WriteLine($"PnL: {custody.PnL}");
                        foreach (var item in custody.Assets)
                        {
                            Console.WriteLine($"Symbol: {item.Value.Symbol} - Qtty: {item.Value.Quantity} - QttyBlocked: {item.Value.QuantityBlocked} - Value: {item.Value.Value}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine($"[NGrains] - GetCustody elapsed time: {elapsedTime / ccis}ms - Continue? (y/n):");

                again = Console.ReadLine();

            } while (again == "y");
        }

        public async Task BlockAsset(int cci, string symbol)
        {
            while (true)
            {
                try
                {
                    Console.Clear();

                    Console.WriteLine("-----------");
                    Console.WriteLine("[BlockAsset]");
                    Console.WriteLine("-----------");
                    _stopwatch.Restart();
                    var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                    var blocked = await custodyGrain.BlockAssetIfAvailable(symbol, 1);
                    _stopwatch.Stop();
                    Console.WriteLine($"[BlockAsset] - Cluster elapsed time: {_stopwatch.ElapsedMilliseconds}ms");

                    var custody = await custodyGrain.GetCustody();
                    Console.WriteLine($"CCI: {cci}");
                    Console.WriteLine($"PnL: {custody.PnL}");
                    foreach (var item in custody.Assets)
                    {
                        Console.WriteLine($"Symbol: {item.Value.Symbol} - Qtty: {item.Value.Quantity} - QttyBlocked: {item.Value.QuantityBlocked} - Value: {item.Value.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay(10);
            }
        }

        public async Task AddAsset(int cci)
        {
            try
            {
                Console.WriteLine("-----------");
                Console.WriteLine("[AddAsset]");
                Console.WriteLine("-----------");
                Console.Clear();
                _stopwatch.Restart();
                var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                await custodyGrain.AddAsset(new Asset
                {
                    Value = 0,
                    BlockedValue = 0,
                    Quantity = 1000,
                    QuantityBlocked = 0,
                    Symbol = "OIBR3"
                });
                _stopwatch.Stop();
                Console.WriteLine($"[AddAsset] - Cluster elapsed time: {_stopwatch.ElapsedMilliseconds}ms");

                var custody = await custodyGrain.GetCustody();
                Console.WriteLine($"CCI: {cci}");
                Console.WriteLine($"PnL: {custody.PnL}");
                foreach (var item in custody.Assets)
                {
                    Console.WriteLine($"Symbol: {item.Value.Symbol} - Qtty: {item.Value.Quantity} - QttyBlocked: {item.Value.QuantityBlocked} - Value: {item.Value.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task PoolingAGrain(int cci)
        {
            try
            {
                Console.WriteLine("-----------");
                Console.WriteLine("[PoolingAGrain]");
                Console.WriteLine("-----------");
                while (true)
                {
                    Console.Clear();
                    _stopwatch.Restart();
                    var custodyGrain = _client.GetGrain<ICustodyGrain>(cci);
                    var custody = await custodyGrain.GetCustody();
                    _stopwatch.Stop();
                    Console.WriteLine($"[PoolingAGrain] - Cluster elapsed time: {_stopwatch.ElapsedMilliseconds}ms");

                    Console.WriteLine($"CCI: {cci}");
                    Console.WriteLine($"PnL: {custody.PnL}");
                    foreach (var item in custody.Assets)
                    {
                        Console.WriteLine($"Symbol: {item.Value.Symbol} - Qtty: {item.Value.Quantity} - QttyBlocked: {item.Value.QuantityBlocked} - Value: {item.Value.Value}");
                    }

                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task StreamingAGrain(int cci)
        {
            try
            {
                Console.WriteLine("-----------");
                Console.WriteLine("[StreamingAGrain]");
                Console.WriteLine("-----------");

                var streamProvider = _client.GetStreamProvider("LivePnLStreamProvider");

                var custody = _client.GetGrain<ICustodyGrain>(cci);

                var stream = streamProvider.GetStream<decimal>(StreamId.Create("PnL", cci));

                await stream.SubscribeAsync<decimal>(OnPnL);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task OnPnL(decimal pnl, StreamSequenceToken? token = null)
        {
            Console.Clear();
            Console.WriteLine($"Pnl: {pnl}");
            await Task.Delay(1);
        }
    }
}
