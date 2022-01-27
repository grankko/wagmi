using System.Globalization;
using System.Text.Json.Nodes;
using Wagmi.Cli.Model;
using System.Linq;

namespace Wagmi.Cli
{
    internal class Program
    {
        const int LowMA = 15;
        const int HighMA = 45;

        static void Main(string[] args)
        {
            var candles = new List<Candle>();
            candles.AddRange(ParseJson(@"E:\Code\Wagmi\Data\BTC2.json"));
            candles.AddRange(ParseJson(@"E:\Code\Wagmi\Data\BTC.json"));
            var orderedCandles = candles.OrderBy(c => c.Time).ToList();

            CalculateMovingAverages(candles);

            FindMovingAverageFlips(candles);

            Console.ReadLine();
        }

        private static void FindMovingAverageFlips(List<Candle> candles)
        {
            for (int i = HighMA; i < candles.Count; i++)
            {
                var currentCandle = candles[i];
                var previousCandle = candles[i - 1];

                if (previousCandle.CurrentPosition == Position.Short && currentCandle.CurrentPosition == Position.Long)
                {
                    Console.WriteLine($"{currentCandle.Time.ToShortDateString()} - Buying at price {currentCandle.Average}");
                }
                else if (previousCandle.CurrentPosition == Position.Long && currentCandle.CurrentPosition == Position.Short)
                {
                    Console.WriteLine($"{currentCandle.Time.ToShortDateString()} - Selling at price {currentCandle.Average}");
                }
            }
        }

        private static void CalculateMovingAverages(List<Candle> candles)
        {
            for (int i = HighMA; i < candles.Count; i++)
            {
                var currentCandle = candles[i];

                int n = 0;
                while (n < LowMA)
                {
                    currentCandle.LowMA.AddCandle(candles[i - n]);
                    n++;
                }
                n = 0;
                while (n < HighMA)
                {
                    currentCandle.HighMA.AddCandle(candles[i - n]);
                    n++;
                }

                if (currentCandle.LowMA.Value > currentCandle.HighMA.Value)
                    currentCandle.CurrentPosition = Position.Long;
                else
                    currentCandle.CurrentPosition = Position.Short;
            }
        }

        private static List<Candle> ParseJson(string path)
        {
            var jsonString = File.ReadAllText(path);
            var jsonObject = JsonNode.Parse(jsonString);

            var candles = new List<Candle>();

            foreach (var candleObject in jsonObject["Data"]["Data"].AsArray())
            {
                var candle = new Candle(LowMA, HighMA)
                {
                    Close = candleObject["close"].GetValue<double>(),
                    High = candleObject["high"].GetValue<double>(),
                    Low = candleObject["low"].GetValue<double>(),
                    Time = DateTimeOffset.FromUnixTimeSeconds(candleObject["time"].GetValue<int>()).DateTime
                };

                candle.Average = (candle.High + candle.Low) / 2;
                candles.Add(candle);
            }

            return candles;
        }
    }
}