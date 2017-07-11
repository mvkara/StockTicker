using System;

namespace NabStockTickerSource.Util
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private Random randomGenerator;

        public RandomNumberGenerator()
        {
            this.randomGenerator = new Random();
        }

        public double NextRandom()
        {
            return this.randomGenerator.NextDouble();
        }
    }
}