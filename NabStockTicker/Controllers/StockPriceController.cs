using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NabStockTickerSource;
using NabStockTickerSource.DomainObjects;

namespace NabStockTicker.Controllers
{
    public class StockPriceController : Controller
    {
        public const string StockPricePropertyName = "StockPrices";
        private readonly IStockTickerSource stockTickerSource;

        public StockPriceController(IStockTickerSource stockTickerSource)
        {
            this.stockTickerSource = stockTickerSource;
        }

        public IActionResult Index(string searchString)
        {
            StockPriceUpdate stockPriceUpdate;

            if (String.IsNullOrEmpty(searchString))
            {
                ViewData[StockPricePropertyName] = this.stockTickerSource.CurrentPrices;
            }
            else if (this.stockTickerSource.TryGetLastStockPriceByFeedCode(searchString, out stockPriceUpdate))
            {
                ViewData[StockPricePropertyName] = new[] { stockPriceUpdate };
            }
            else
            {
                ViewData[StockPricePropertyName] = Enumerable.Empty<StockPriceUpdate>();
            }
            
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "NAB Stock Ticker Page";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Author: mvkra";

            return View();
        }
    }
}
