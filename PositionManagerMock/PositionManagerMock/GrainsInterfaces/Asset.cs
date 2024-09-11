using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsInterfaces
{
    [GenerateSerializer, Immutable]
    public class Asset
    {
        [Id(0)]
        public decimal BlockedValue { get; set; }
        [Id(1)]
        public decimal Value { get; set; }
        [Id(2)]
        public decimal Quantity { get; set; }
        [Id(3)]
        public decimal QuantityBlocked { get; set; }
        [Id(4)]
        public string Symbol { get; set; }

        [Id(5)]
        public decimal GetAvaliableQtty()
        {
            return this.Quantity - this.QuantityBlocked;
        }
    }
}
