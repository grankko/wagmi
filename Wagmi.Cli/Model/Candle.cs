using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wagmi.Cli.Model
{
    internal class Candle
    {
        public DateTime Time { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Average { get; set; }

        public MovingAverage LowMA { get; private set; }
        public MovingAverage HighMA { get; private set; }

        public Position CurrentPosition { get; set; }

        public Candle(int lowMaDays, int highMaDays)
        {
            LowMA = new MovingAverage(lowMaDays);
            HighMA = new MovingAverage(highMaDays);
            CurrentPosition = Position.Undecided;
        }
    }

    internal enum Position
    {
        Undecided,
        Long,
        Short
    }
}