using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Live.Modes;

internal abstract class BaseRun
{
    private readonly IStrategy strategy;

    private readonly IExchangeDataframeProvider dataframeProvider;

    protected IExchange ExchangeInterface { get; private set; }

    protected ILogger? Logger { get; private set; }

    public BaseRun(Pair pair, Timeframe timeframe, IStrategy strategy, IExchange exchange, ILogger? logger = null)
    {
        dataframeProvider = DataframeProviderFactory.GetExchangeDataframeProvider(Exchange.Kucoin, timeframe, pair);
        this.strategy = strategy;
        ExchangeInterface = exchange;
        Logger = logger;
    }

    public async Task RunAsync(int preloadCandles = 0)
    {
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => Logger?.LogDebug("{p}", p.Description));
        try
        {
            await dataframeProvider.LoadAndPreloadCandles(preloadCandles, progress);
            strategy.OnStart();
            DataFrame? frame;
            while ((frame = await dataframeProvider.Next()) is not null)
            {
                await strategy.UpdateState(frame);
                Heartbeat(frame);
            }
            strategy.OnStop();
        }
        catch (Exception e)
        {
            Logger?.LogError("Exception captured: {e}", e);
        }
    }

    /// <summary>
    ///   Called when a kline is received, after updating the strategy
    /// </summary>
    protected virtual async void Heartbeat(DataFrame frame) => await Task.CompletedTask;
}