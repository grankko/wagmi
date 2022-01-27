using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wagmi.Cli.Model
{
    internal class MovingAverage
    {
        private List<Candle> History { get; set; }
        public int MovingAverageDays { get; private set; }
        public double Value { get {
                return History.Average(c => c.Average);
            }
        }

        public MovingAverage(int days)
        {
            MovingAverageDays = days;
            History = new List<Candle>();
        }

        public void AddCandle(Candle candle)
        {
            History = History.OrderBy(c => c.Time).ToList();

            if (History.Count >= MovingAverageDays)
                History.RemoveAt(0);

            History.Add(candle);
        }
    }
}
