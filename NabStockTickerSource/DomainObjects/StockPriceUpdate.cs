using System;
using System.Collections.Generic;
using System.Text;

namespace NabStockTickerSource.DomainObjects
{
    /// <summary>
    /// A stock price update message
    /// </summary>
    public class StockPriceUpdate
    {
        public StockPriceUpdate(string feedCode, double price)
        {
            this.FeedCode = feedCode;
            this.Price = price;
        }

        public string FeedCode { get; private set; }

        public double Price { get; private set; }
    }
}
