using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsInterfaces
{
    [GenerateSerializer, Immutable]
    public class Custody
    {
        [Id(0)]
        public Dictionary<string, Asset> Assets { get; set; }
        [Id(1)]
        public decimal PnL { get; set; }
        [Id(2)]
        public decimal TotalBlockedValue { get; set; }
        public Custody()
        {
            Assets = new Dictionary<string, Asset>();
        }
    }
}
