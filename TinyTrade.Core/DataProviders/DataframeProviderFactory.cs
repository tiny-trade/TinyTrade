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
    public static BacktestDataframeProvider GetBacktestDataframeProvider(TimeInterval interval, Pair pair, Timeframe timeframe)
        => new BacktestDataframeProvider(interval, pair, timeframe);

    public static ParallelBacktestDataframeProvider GetParallelBacktestDataframeProvider(TimeInterval interval, Pair pair, Timeframe timeframe)
        => new ParallelBacktestDataframeProvider(interval, pair, timeframe);

    public static IExchangeDataframeProvider GetExchangeDataframeProvider(Exchange exchange, Timeframe timeframe, Pair pair)
    {
        return exchange switch
        {
            Exchange.Kucoin => new KucoinDataframeProvider(pair, timeframe),
            _ => null!,
        };
    }
}