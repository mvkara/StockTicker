using NabStockTickerSource.Util;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Reactive.Testing;
using NabStockTickerSource.DomainObjects;

namespace NabStockTickerSource.Unit
{
    [TestFixture]
    public class StockTickerSourceTest
    {
        private readonly long pollingInterval = 1;

        private Mock<IInitialDataGenerator> initialDataGeneratorMock;
        private Mock<IRandomNumberGenerator> randomNumberGeneratorMock;
        private TestScheduler testScheduler;

        private readonly StockPriceUpdate[] testData = new[]
        {
            new StockPriceUpdate("ABC", 1.0),
            new StockPriceUpdate("DEF", 20.0),
        };

        [SetUp]
        public void Setup()
        {
            this.initialDataGeneratorMock = new Mock<IInitialDataGenerator>();
            this.randomNumberGeneratorMock = new Mock<IRandomNumberGenerator>();
            this.testScheduler = new TestScheduler();
            this.initialDataGeneratorMock.Setup(x => x.GenerateStocks()).Returns(testData);
        }

        [Test]
        public void InitialDataLoaded_ReturnedValuesAreEqual()
        {
            using (var stockTickerSource = this.BuildSystemUnderTest())
            {
                Assert.That(stockTickerSource.CurrentPrices, Is.EquivalentTo(this.testData).Using(new StockPriceUpdateEqualityComparer()));
            }
        }

        [Test]
        public void InitialDataLoadComplete_TicksChangePricesInBothPositiveAndNegativeDirections()
        {
            using (var stockTickerSource = this.BuildSystemUnderTest())
            {
                Assert.That(stockTickerSource.CurrentPrices, Is.EquivalentTo(this.testData).Using(new StockPriceUpdateEqualityComparer()));

                var randomNumberSequence = new double[] { 4, 5, 6, 7 };
                // Two stocks polled twice so 4 numbers required
                var numberQueue = new Queue<double>(new double[] { 4, 5, 6, 7 });
                this.randomNumberGeneratorMock.Setup(x => x.NextRandom()).Returns(numberQueue.Dequeue);

                this.testScheduler.AdvanceBy(pollingInterval);

                // Price can never fall below zero
                var currentStockAAfterDecrement = new StockPriceUpdate(testData[0].FeedCode, 0);
                var currentStockBAfterDecrement = new StockPriceUpdate(testData[1].FeedCode, testData[1].Price - randomNumberSequence[1]);

                Assert.That(stockTickerSource.CurrentPrices, Is.EquivalentTo(new[] { currentStockAAfterDecrement, currentStockBAfterDecrement }).Using(new StockPriceUpdateEqualityComparer()));

                this.testScheduler.AdvanceBy(pollingInterval);

                var currentStockAAfterIncrement = new StockPriceUpdate(testData[0].FeedCode, currentStockAAfterDecrement.Price + randomNumberSequence[2]);
                var currentStockBAfterIncrement = new StockPriceUpdate(testData[1].FeedCode, currentStockBAfterDecrement.Price + randomNumberSequence[3]);

                Assert.That(stockTickerSource.CurrentPrices, Is.EquivalentTo(new[] { currentStockAAfterIncrement, currentStockBAfterIncrement }).Using(new StockPriceUpdateEqualityComparer()));
            }
        }

        [Test]
        public void InitialDataLoadComplete_TryGetPriceReturnsTrueForPriceInStore()
        {
            using (var stockTickerSource = this.BuildSystemUnderTest())
            {
                StockPriceUpdate stockPriceUpdate;
                Assert.IsTrue(stockTickerSource.TryGetLastStockPriceByFeedCode(testData[0].FeedCode, out stockPriceUpdate));
                Assert.That(stockPriceUpdate, Is.EqualTo(testData[0]).Using(new StockPriceUpdateEqualityComparer()));
            }
        }

        public void InitialDataLoadComplete_TryGetPriceReturnsFalseForPriceNotInStore()
        {
            using (var stockTickerSource = this.BuildSystemUnderTest())
            {
                StockPriceUpdate stockPriceUpdate;
                Assert.IsFalse(stockTickerSource.TryGetLastStockPriceByFeedCode("INVALID", out stockPriceUpdate));
                Assert.That(stockPriceUpdate, Is.Null);
            }
        }

        private StockTickerSource BuildSystemUnderTest()
        {
            return new StockTickerSource(initialDataGeneratorMock.Object, randomNumberGeneratorMock.Object, testScheduler, TimeSpan.FromTicks(pollingInterval));
        }

        private class StockPriceUpdateEqualityComparer : IEqualityComparer<StockPriceUpdate>
        {
            public bool Equals(StockPriceUpdate x, StockPriceUpdate y)
            {
                return String.Equals(x.FeedCode, y.FeedCode) && x.Price == y.Price;
            }

            public int GetHashCode(StockPriceUpdate obj)
            {
                // Only needed in test; performance isn't critical.
                return 1;
            }
        }
    }
}
