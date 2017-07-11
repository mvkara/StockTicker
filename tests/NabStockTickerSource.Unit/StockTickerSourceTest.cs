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
        private readonly TimeSpan pollingInterval = TimeSpan.FromTicks(1);

        private Mock<IInitialDataGenerator> initialDataGeneratorMock;
        private TestScheduler testScheduler;

        private readonly StockPriceUpdate[] testData = new[]
        {
            new StockPriceUpdate("ABC", 1.0),
            new StockPriceUpdate("DEF", 2.0),
        };

        [SetUp]
        public void Setup()
        {
            this.initialDataGeneratorMock = new Mock<IInitialDataGenerator>();
            this.testScheduler = new TestScheduler();
            this.initialDataGeneratorMock.Setup(x => x.GenerateStocks()).Returns(testData);
        }

        [Test]
        public void InitialDataLoaded_ReturnedValuesAreEqual()
        {
            this.initialDataGeneratorMock.Setup(x => x.GenerateStocks()).Returns(testData);

            using (var stockTickerSource = new StockTickerSource(initialDataGeneratorMock.Object, testScheduler, pollingInterval))
            {
                Assert.That(stockTickerSource.CurrentPrices, Is.EquivalentTo(this.testData).Using(new StockPriceUpdateEqualityComparer()));
            }
        }

        [Test]
        public void InitialDataLoadComplete_OneRandomTick_ValuesChanged()
        {
             
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
