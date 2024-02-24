/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Util;
using System;
using QuantConnect.Configuration;
using System.Linq;
using static QuantConnect.Configuration.ApplicationParser;

namespace QuantConnect.CoinbaseBrokerage.ToolBox
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var optionsObject = ToolboxArgumentParser.ParseArguments(args);
            if (optionsObject.Count == 0)
            {
                PrintMessageAndExit();
            }

            if (!optionsObject.TryGetValue("app", out var targetApp))
            {
                PrintMessageAndExit(1, "ERROR: --app value is required");
            }

            if(string.IsNullOrEmpty(Config.GetValue<string>("coinbase-api-key")) || string.IsNullOrEmpty(Config.GetValue<string>("coinbase-api-secret")))
            {
                PrintMessageAndExit(1, "ERROR: check configs: 'coinbase-api-key' or 'coinbase-api-secret'");
            }

            var targetAppName = targetApp.ToString();
            if (targetAppName.Contains("download") || targetAppName.Contains("dl"))
            {
                var fromDate = Parse.DateTimeExact(GetParameterOrExit(optionsObject, "from-date"), "yyyyMMdd-HH:mm:ss");
                var resolution = optionsObject.ContainsKey("resolution") ? optionsObject["resolution"].ToString() : "";
                var tickers = ToolboxArgumentParser.GetTickers(optionsObject);
                var toDate = optionsObject.ContainsKey("to-date")
                    ? Parse.DateTimeExact(optionsObject["to-date"].ToString(), "yyyyMMdd-HH:mm:ss")
                    : DateTime.UtcNow;

                if (resolution.IsNullOrEmpty() || tickers.Any(s => s.IsNullOrEmpty()))
                {
                    Console.WriteLine($"{nameof(CoinbaseDownloader)}:ERROR: '--tickers=' or '--resolution=' parameter is missing");
                    Console.WriteLine("--tickers=ETHUSD,ETHBTC,BTCUSD,etc.");
                    Console.WriteLine("--resolution=Second/Minute/Hour/Daily");
                    Environment.Exit(1);
                }

                CoinbaseDownloaderProgram.CoinbaseDownloader(tickers, resolution, fromDate, toDate);

                Console.WriteLine("Finish data download. Press any key to continue..");
                Console.ReadLine();
            }
            else if (targetAppName.Contains("updater") || targetAppName.EndsWith("spu"))
            {
                CoinbaseDownloaderProgram.ExchangeInfoDownloader();
            }
            else
            {
                PrintMessageAndExit(1, "ERROR: Unrecognized --app value");
            }
        }
    }
}