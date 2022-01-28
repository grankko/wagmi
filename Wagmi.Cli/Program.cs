using System.Globalization;
using System.Text.Json.Nodes;
using Wagmi.Cli.Model;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Wagmi.Cli
{
    internal class Program
    {
        private static int CurrentLowMa;
        private static int CurrentHighMa;

        // Config values
        private static int InitialInvestment;
        private static int LowMaStart;
        private static int LowMaEnd;
        private static int HighMaStart;
        private static int HighMaEnd;
        private static bool SaveOutput;

        static void Main(string[] args)
        {
            InitConfig();

            Console.WriteLine($"Low moving average range: {LowMaStart} - {LowMaEnd}");
            Console.WriteLine($"High moving average range: {HighMaStart} - {HighMaEnd}");
            Console.WriteLine($"Investing {InitialInvestment.ToString("N2")} USD");
            Console.WriteLine();
            Console.WriteLine("Press enter to start..");
            Console.WriteLine();

            Console.ReadLine();

            var candles = new List<Candle>();

            candles.AddRange(ParseJson(@"Data/BTC2.json"));
            candles.AddRange(ParseJson(@"Data/BTC.json"));
            var orderedCandles = candles.OrderBy(c => c.Time).ToList();

            var investmentResults = new List<InvestmentResult>();

            for (int lowMa = LowMaStart; lowMa <= LowMaEnd; lowMa++)
            {
                for (int highMa = HighMaStart; highMa <= HighMaEnd; highMa++)
                {
                    CurrentLowMa = lowMa;
                    CurrentHighMa = highMa;

                    CalculateMovingAverages(candles);
                    var result = RunAnalysis(candles);
                    investmentResults.Add(result);

                    Console.WriteLine($"Analyzed {result.LowMa}/{result.HighMa}. Yield: {result.Usd.ToString("N2")} USD - {result.Btc.ToString("N2")} BTC");
                    Console.WriteLine();

                }
            }

            var topResult = investmentResults.OrderByDescending(i => i.Usd).First();

            Console.WriteLine("===== TOP RESULT =====");
            Console.WriteLine($"Best result: {topResult.Usd.ToString("N2")} USD");
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

            if (SaveOutput)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"LowMa;HighMa;Usd;Btc;Trades");
                foreach (var result in investmentResults)
                    builder.AppendLine($"{result.LowMa};{result.HighMa};{result.Usd.ToString()};{result.Btc.ToString()};{result.Trades.Count()}");

                File.WriteAllText("out.csv", builder.ToString());

                Console.WriteLine("Summary saved to out.csv");
            }

            Console.ReadLine();
        }

        private static InvestmentResult RunAnalysis(List<Candle> candles)
        {
            double usdHolding = InitialInvestment;
            double btcHolding = 0;

            var investmentResult = new InvestmentResult(CurrentLowMa, CurrentHighMa);

            for (int i = CurrentHighMa; i < candles.Count; i++)
            {
                var currentCandle = candles[i];
                var previousCandle = candles[i - 1];

                if (previousCandle.CurrentPosition == Position.Short && currentCandle.CurrentPosition == Position.Long)
                {
                    btcHolding = usdHolding / currentCandle.Close;
                    usdHolding = 0;

                    var trade = new Trade() { BtcAquired = btcHolding, UsdAquired = usdHolding, Price = currentCandle.Close, TradeDate = currentCandle.Time, TradeType = TradeType.Buy };
                    investmentResult.Trades.Add(trade);
                }
                else if (previousCandle.CurrentPosition == Position.Long && currentCandle.CurrentPosition == Position.Short && btcHolding > 0)
                {
                    usdHolding = currentCandle.Close * btcHolding;
                    btcHolding = 0;

                    var trade = new Trade() { BtcAquired = btcHolding, UsdAquired = usdHolding, Price = currentCandle.Close, TradeDate = currentCandle.Time, TradeType = TradeType.Sell };
                    investmentResult.Trades.Add(trade);
                }
            }

            investmentResult.Usd = usdHolding;
            investmentResult.Btc = btcHolding;

            return investmentResult;
        }

        private static void CalculateMovingAverages(List<Candle> candles)
        {
            for (int i = CurrentHighMa; i < candles.Count; i++)
            {
                var currentCandle = candles[i];
                currentCandle.InitializeMa(CurrentLowMa, CurrentHighMa);

                int n = 0;
                while (n < CurrentLowMa)
                {
                    currentCandle.LowMa.AddCandle(candles[i - n]);
                    n++;
                }
                n = 0;
                while (n < CurrentHighMa)
                {
                    currentCandle.HighMa.AddCandle(candles[i - n]);
                    n++;
                }

                if (currentCandle.LowMa.Value > currentCandle.HighMa.Value)
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

        private static void InitConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build();

            LowMaStart = config.GetRequiredSection("lowMaRange").GetValue<int>("start");
            LowMaEnd = config.GetRequiredSection("lowMaRange").GetValue<int>("end");

            HighMaStart = config.GetRequiredSection("highMaRange").GetValue<int>("start");
            HighMaEnd = config.GetRequiredSection("highMaRange").GetValue<int>("end");

            InitialInvestment = config.GetRequiredSection("initialInvestment").Get<int>();
            SaveOutput = config.GetRequiredSection("saveOutput").Get<bool>();
        }
    }
}