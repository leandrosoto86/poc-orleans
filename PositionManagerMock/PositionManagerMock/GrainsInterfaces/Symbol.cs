using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsInterfaces
{
    [GenerateSerializer, Immutable]
    public class Symbol
    {
        [Id(0)]
        public string Ticker { get; set; }
        [Id(1)]
        public decimal LastQuote { get; set; }
    }
}
