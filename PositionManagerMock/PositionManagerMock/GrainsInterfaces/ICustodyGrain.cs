using Orleans;
using Orleans.Streams;

namespace GrainsInterfaces
{
    [Alias("GrainsInterfaces.ICustodyGrain")]
    public interface ICustodyGrain : IGrainWithIntegerKey
    {
        ValueTask CreateRandomCustody();
        ValueTask AddAsset(Asset asset);
        ValueTask<Custody> GetCustody();
        ValueTask<Asset?> GetAsset(string symbol);
        ValueTask<decimal> GetPnL();
        ValueTask<bool> BlockAssetIfAvailable(string symbol, decimal qtty);
    }
}
