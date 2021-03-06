using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCoreAudio;

namespace BinanceAlarm
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var client = new HttpClient();

            CultureInfo info = new CultureInfo("tr-TR");
            info.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = info;
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            while (true)
            {
                Console.WriteLine($"Binance verileri kontrol ediliyor. {DateTime.Now}");
                Console.WriteLine($"-----------------------------------------------------------");

                var anyRuleDone = false;

                var result = await client.GetStringAsync("https://www.binance.com/api/v3/ticker/price");

                var marketDataList = JsonSerializer.Deserialize<List<MarketPriceModel>>(result);

                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"rules.txt");
                var lines = File.ReadAllLines(path);

                foreach (var line in lines)
                {
                    var lineArray = line.Split(" ");
                    var symbol = lineArray[0];
                    var rule = lineArray[1];
                    var price = double.Parse(lineArray[2].Replace(",", ""), info);

                    var marketData = marketDataList.FirstOrDefault(x => x.symbol == symbol);

                    if (marketData != null)
                    {
                        var marketPrice = double.Parse(marketData.price, info);

                        if (rule == ">")
                        {

                            if (marketPrice >= price)
                            {
                                PlayAlarm();

                                Console.WriteLine($"{marketData.symbol} fiyatı {marketData.price} oldu. {DateTime.Now}");
                                Console.WriteLine($"-----------------------------------------------------------");
                                anyRuleDone = true;
                            }
                        }
                        else
                        {
                            if (marketPrice <= price)
                            {
                                PlayAlarm();

                                Console.WriteLine($"{marketData.symbol} fiyatı {marketData.price} oldu. {DateTime.Now}");
                                Console.WriteLine($"-----------------------------------------------------------");
                                anyRuleDone = true;
                            }
                        }
                    }
                }

                if (!anyRuleDone)
                {
                    Console.WriteLine($"Gerçekleşen koşul yok. {DateTime.Now}");
                    Console.WriteLine($"-----------------------------------------------------------");
                }

                Thread.Sleep(60000);
            }
        }

        public static void PlayAlarm()
        {
            string alarmPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"alarm.wav");
            var player = new Player();
            player.Play(alarmPath).Wait();
        }
    }
}
