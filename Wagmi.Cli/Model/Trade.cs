using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wagmi.Cli.Model
{
    internal class Trade
    {
        public TradeType TradeType { get; set; }
        public double Price { get; set; }
        public DateTime TradeDate { get; set; }

        public double BtcAquired { get; set; }
        public double UsdAquired { get; set; }

    }

    internal enum TradeType
    {
        Sell,
        Buy
    }

}
