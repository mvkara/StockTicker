using System;
using NabStockTickerSource.DomainObjects;
using System.Collections.Generic;

namespace NabStockTickerSource
{
    public interface IStockTickerSource : IDisposable
    {
        ICollection<StockPriceUpdate> CurrentPrices { get; }

        bool TryGetLastStockPriceByFeedCode(string feedCode, out StockPriceUpdate source);
    }
}
