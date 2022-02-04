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
        public double Average { get => (Low + High) / 2;  }

        public MovingAverage? LowMa { get; private set; }
        public MovingAverage? HighMa { get; private set; }

        public Position CurrentPosition { get; set; }

        public void InitializeMa(int lowMaDays, int highMaDays)
        {
            LowMa = new MovingAverage(lowMaDays);
            HighMa = new MovingAverage(highMaDays);
            CurrentPosition = Position.Undecided;
        }

        public override string ToString()
        {
            return $"{Time.ToShortDateString} - {Average}";
        }
    }

    internal enum Position
    {
        Undecided,
        Long,
        Short
    }
}