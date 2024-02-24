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
using System;
using System.Linq;
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Util;
using QuantConnect.Logging;
using System.Globalization;
using QuantConnect.ToolBox;
using System.Collections.Generic;

namespace QuantConnect.CoinbaseBrokerage.ToolBox
{
    public static class CoinbaseDownloaderProgram
    {
        /// <summary>
        /// Coinbase Downloader Toolbox Project For LEAN Algorithmic Trading Engine.
        /// </summary>
        public static void CoinbaseDownloader(IList<string> tickers, string resolution, DateTime fromDate, DateTime toDate)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var castResolution = (Resolution) Enum.Parse(typeof(Resolution), resolution);
            try
            {
                // Load settings from config.json
                var dataDirectory = Globals.DataFolder;
                //todo: will download any exchange but always save as coinbase
                // Create an instance of the downloader
                const string market = Market.Coinbase;
                var downloader = new CoinbaseDownloader();
                foreach (var ticker in tickers)
                {
                    Log.Trace("Start data download for " + ticker);
                    // Download the data
                    var symbolObject = Symbol.Create(ticker, SecurityType.Crypto, market);
                    var data = downloader.Get(new DataDownloaderGetParameters(symbolObject, castResolution, fromDate, toDate));

                    // Save the data
                    var writer = new LeanDataWriter(castResolution, symbolObject, dataDirectory, TickType.Trade);
                    var distinctData = data.GroupBy(i => i.Time, (key, group) => group.First()).ToArray();

                    writer.Write(distinctData);
                }

                Log.Trace("Finish data download");

            }
            catch (Exception err)
            {
                Log.Error(err);
                Log.Trace(err.Message);
                Log.Trace(err.StackTrace);
            }
        }

        /// <summary>
        /// Endpoint for downloading exchange info
        /// </summary>
        public static void ExchangeInfoDownloader()
        {
            new ExchangeInfoUpdater(new CoinbaseExchangeInfoDownloader())
                .Run();
        }
    }
}
