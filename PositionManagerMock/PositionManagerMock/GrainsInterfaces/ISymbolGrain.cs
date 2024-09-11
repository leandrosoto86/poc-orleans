using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsInterfaces
{
    [Alias("GrainsInterfaces.ISymbolGrain")]
    public interface ISymbolGrain : IGrainWithStringKey
    {
        ValueTask<Symbol> GetSymbol();
        ValueTask SetLastQuote(decimal lastQuote);
        ValueTask<decimal> GetLastQuote();
    }
}
