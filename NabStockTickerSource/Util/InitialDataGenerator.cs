using NabStockTickerSource.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace NabStockTickerSource.Util
{
    /// <summary>
    /// Generates the initial data for the stock ticker.
    /// Mockable out so that we can generate the data ourselves in tests.
    /// </summary>
    public class InitialDataGenerator : IInitialDataGenerator
    {
        public const int NumberOfStocks = 50;

        public IEnumerable<StockPriceUpdate> GenerateStocks()
        {
            Random random = new Random();

            for (int i = 0; i < NumberOfStocks; i++)
            {
                // Using numbers as feedcodes - some markets do this (XOSE)
                yield return new StockPriceUpdate(i.ToString(), random.NextDouble() * 100);
            }
        }
    }
}
