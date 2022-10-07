using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.DataProviders;

public static class DataframeProviderFactory
{
    /// <summary>
    /// </summary>
    /// <param name="interval"> </param>
    /// <param name="pair"> </param>
    /// <param name="initialBalance"> </param>
    /// <param name="logger"> </param>
    /// <returns> A dataframe provider that uses backtest data saved on disk </returns>
    public static BacktestDataframeProvider GetBacktestDataframeProvider(TimeInterval interval, Pair pair, int initialBalance = 1000)
        => new BacktestDataframeProvider(interval, pair, initialBalance);

    public static IExchangeDataframeProvider GetExchangeDataframeProvider(Exchange exchange, Timeframe timeframe, Pair pair)
    {
        return exchange switch
        {
            Exchange.Kucoin => new KucoinDataframeProvider(pair, timeframe),
            _ => null!,
        };
    }
}