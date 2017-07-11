using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NabStockTickerSource.DomainObjects;
using NabStockTickerSource.Util;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace NabStockTickerSource
{
    /// <summary>
    /// A mock source generating data as expected.
    /// Thread-safe from multiple callers.
    /// </summary>
    public class StockTickerSource : IStockTickerSource
    {
        private const double PriceChangeRateInSeconds = 5.0;

        private Dictionary<string, StockPriceUpdate> lastStockPriceByFeedCode;
        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private IDisposable timerSubscription;
        private bool disposedValue = false; 

        // TODO: In a real app these would come from a config file.
        // If I get time figure out how ASP.NET gets configuration and how to wire that
        // up to its dependency injection framework.
        public StockTickerSource()
            : this(new InitialDataGenerator(), TaskPoolScheduler.Default, TimeSpan.FromSeconds(PriceChangeRateInSeconds))
        {
        }

        // RAII - Acquiring the resource is its initialisation.
        public StockTickerSource(IInitialDataGenerator dataGenerator, IScheduler timeScheduler, TimeSpan pollingInterval)
        {
            this.lastStockPriceByFeedCode = dataGenerator.GenerateStocks().ToDictionary(x => x.FeedCode, x => x);

            this.timerSubscription =
                Observable.Interval(pollingInterval, timeScheduler)
                .Scan(true, (prev, _) => !prev)
                .Subscribe(upOrDown => this.GeneratePricesMovement(upOrDown));
        }

        public ICollection<StockPriceUpdate> CurrentPrices
        {
            get
            {
                if (this.disposedValue)
                {
                    throw new ObjectDisposedException("StockTickerSource has been disposed, operation not permitted");
                }

                this.readerWriterLockSlim.EnterReadLock();
                var result = this.lastStockPriceByFeedCode.Values;
                this.readerWriterLockSlim.ExitReadLock();
                return result;
            }
        }

        public bool TryGetLastStockPriceByFeedCode(string feedCode, out StockPriceUpdate source)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException("StockTickerSource has been disposed, operation not permitted");
            }

            this.readerWriterLockSlim.EnterReadLock();
            var result = this.lastStockPriceByFeedCode.TryGetValue(feedCode, out source);
            this.readerWriterLockSlim.ExitReadLock();
            return result;
        }

        private void GeneratePricesMovement(bool isDirectionUp)
        {
            Random random = new Random();

            // I'm assuming as part of the spec that we can't update the prices one by one and we
            // need consistency across prices updated so using a lock here.
            this.readerWriterLockSlim.EnterWriteLock();

            var feedCodes = new List<string>(this.lastStockPriceByFeedCode.Keys);
            foreach (var feedcode in feedCodes)
            {
                var previousPrice = this.lastStockPriceByFeedCode[feedcode];

                var newPrice =
                    isDirectionUp
                    ? previousPrice.Price + random.NextDouble()
                    : Math.Max(0, previousPrice.Price - random.NextDouble());

                this.lastStockPriceByFeedCode[feedcode] = new StockPriceUpdate(feedcode, newPrice);
            }

            this.readerWriterLockSlim.ExitWriteLock();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.timerSubscription.Dispose();
                    this.readerWriterLockSlim.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
