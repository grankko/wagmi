using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wagmi.Cli.Model
{
    internal class InvestmentResult
    {
        public double Usd { get; set; }
        public double Btc { get; set; }

        public int LowMa { get; private set; }
        public int HighMa { get; set; }
        public List<Trade> Trades { get; set; }


        public InvestmentResult(int lowMa, int highMa)
        {
            LowMa = lowMa;
            HighMa = highMa;

            Trades = new List<Trade>();
        }
    }
}
