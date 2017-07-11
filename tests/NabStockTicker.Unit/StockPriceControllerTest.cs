using NabStockTickerSource;
using NUnit.Framework;
using Moq;
using System;
using NabStockTickerSource.DomainObjects;
using NabStockTicker.Controllers;
using System.Collections.Generic;

namespace NabStockTicker.Unit
{
    /// <summary>
    /// Tests <see cref="StockPriceController"/>
    /// </summary>
    public class StockPriceControllerTest
    {
        private readonly StockPriceUpdate[] testData = new[]
        {
            new StockPriceUpdate("ABC", 1.0),
            new StockPriceUpdate("DEF", 2.0),
        };

        private Mock<IStockTickerSource> stockTickerSourceMock;
        private StockPriceController stockController;

        [SetUp]
        public void Setup()
        {
            this.stockTickerSourceMock = new Mock<IStockTickerSource>(MockBehavior.Strict);
            this.stockController = new StockPriceController(this.stockTickerSourceMock.Object);
        }

        [Test]
        public void EmptyStringProvided_FetchAll()
        {
            this.stockTickerSourceMock.Setup(x => x.CurrentPrices).Returns(testData);

            this.stockController.Index(String.Empty);

            Assert.That(
                this.stockController.ViewData[StockPriceController.StockPricePropertyName],
                Is.EquivalentTo(this.testData).Using(new StockPriceUpdateEqualityComparer()));

            this.stockTickerSourceMock.Verify(x => x.CurrentPrices);            
        }

        [Test]
        public void ValidFeedCodeStringProvided_FetchOnlyFeedCodePrice()
        {
            StockPriceUpdate update = testData[0];
            this.stockTickerSourceMock.Setup(x => x.TryGetLastStockPriceByFeedCode(testData[0].FeedCode, out update)).Returns(true);

            this.stockController.Index(update.FeedCode);

            Assert.That(
                this.stockController.ViewData[StockPriceController.StockPricePropertyName],
                Is.EquivalentTo(new[] { update }).Using(new StockPriceUpdateEqualityComparer()));
        }

        [Test]
        public void InvalidFeedCodeStringProvided_FetchNone()
        {
            String invalidFeedCode = "INVALID";
            StockPriceUpdate update = testData[0];
            this.stockTickerSourceMock.Setup(x => x.TryGetLastStockPriceByFeedCode(invalidFeedCode, out update)).Returns(false);

            this.stockController.Index(invalidFeedCode);

            Assert.That(
                this.stockController.ViewData[StockPriceController.StockPricePropertyName],
                Is.Empty);
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
