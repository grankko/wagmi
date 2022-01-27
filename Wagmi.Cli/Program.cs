using System.Globalization;
using System.Text.Json.Nodes;
using Wagmi.Cli.Model;
using System.Linq;

namespace Wagmi.Cli
{
    internal class Program
    {
        private static int LowMA = 0;
        private static int HighMA = 0;

        static void Main(string[] args)
        {
            var candles = new List<Candle>();
            candles.AddRange(ParseJson(@"E:\Code\Wagmi\Data\BTC2.json"));
            candles.AddRange(ParseJson(@"E:\Code\Wagmi\Data\BTC.json"));
            var orderedCandles = candles.OrderBy(c => c.Time).ToList();

            var investmentResults = new List<InvestmentResult>();

            for (int lowMa = 5; lowMa <= 10; lowMa++)
            {
                for (int highMa = 20; highMa <= 40; highMa++)
                {
                    LowMA = lowMa;
                    HighMA = highMa;

                    CalculateMovingAverages(candles);
                    var result = Invest(candles);
                    investmentResults.Add(result);

                    
                    Console.WriteLine($"Low MA: {result.LowMa}");
                    Console.WriteLine($"High MA: {result.HighMa}");
                    Console.WriteLine($"Result: {result.Usd}");
                    Console.WriteLine();

                }
            }

            var topResult = investmentResults.OrderByDescending(i => i.Usd).First();

            Console.WriteLine("===== TOP RESULT =====");
            Console.WriteLine($"Best result: {topResult.Usd}");
            Console.WriteLine($"Low MA: {topResult.LowMa}");
            Console.WriteLine($"High MA: {topResult.HighMa}");
            Console.WriteLine();
            Console.WriteLine("=====   TRADES   =====");
            foreach (var trade in topResult.Trades)
            {
                if (trade.TradeType == TradeType.Buy)
                    Console.WriteLine($"{trade.TradeDate.ToShortDateString()} - Buying at price {trade.Price}");
                else
                    Console.WriteLine($"{trade.TradeDate.ToShortDateString()} - Selling at price {trade.Price}");

                Console.WriteLine($"You have {trade.UsdAquired.ToString("N2")} USD and {trade.BtcAquired.ToString("N2")} BTC");
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        private static InvestmentResult Invest(List<Candle> candles)
        {
            double usdHolding = 10000;
            double btcHolding = 0;

            var investmentResult = new InvestmentResult(LowMA, HighMA);

            for (int i = HighMA; i < candles.Count; i++)
            {
                var currentCandle = candles[i];
                var previousCandle = candles[i - 1];

                if (previousCandle.CurrentPosition == Position.Short && currentCandle.CurrentPosition == Position.Long)
                {
                    btcHolding = usdHolding / currentCandle.Close;
                    usdHolding = 0;
                    investmentResult.Trades.Add(new Trade() { BtcAquired = btcHolding, UsdAquired = usdHolding, Price = currentCandle.Close, TradeDate = currentCandle.Time });
                }
                else if (previousCandle.CurrentPosition == Position.Long && currentCandle.CurrentPosition == Position.Short && btcHolding > 0)
                {
                    usdHolding = currentCandle.Close * btcHolding;
                    btcHolding = 0;
                    investmentResult.Trades.Add(new Trade() { BtcAquired = btcHolding, UsdAquired = usdHolding, Price = currentCandle.Close, TradeDate = currentCandle.Time });
                }
            }

            investmentResult.Usd = usdHolding;
            investmentResult.Btc = btcHolding;

            return investmentResult;
        }

        private static void CalculateMovingAverages(List<Candle> candles)
        {
            for (int i = HighMA; i < candles.Count; i++)
            {
                var currentCandle = candles[i];
                currentCandle.InitializeMa(LowMA, HighMA);

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
                var candle = new Candle()
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