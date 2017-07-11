namespace NabStockTickerSource.Util
{
    /// <summary>
    /// Generates random numbers for price updates
    /// Mockable out so that we can generate the data ourselves in tests.
    /// </summary>
    public interface IRandomNumberGenerator 
    {
        double NextRandom();
    }
}