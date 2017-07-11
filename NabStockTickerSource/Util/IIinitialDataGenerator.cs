using NabStockTickerSource.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace NabStockTickerSource.Util
{
    /// <summary>
    /// Injectable initial data generator
    /// </summary>
    public interface IInitialDataGenerator
    {
        IEnumerable<StockPriceUpdate> GenerateStocks();
    }
}
