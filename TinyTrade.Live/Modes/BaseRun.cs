using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Statics;

namespace TinyTrade.Live.Modes;

internal enum RunMode
{
    Foretest,
    Live
}

internal class BaseRun
{
    private readonly IExchangeDataframeProvider dataframeProvider;

    private readonly IStrategy strategy;
    private readonly RunMode mode;

    protected StrategyModel StrategyModel { get; private set; }

    protected IExchange ExchangeInterface { get; private set; }

    protected ILogger? Logger { get; private set; }

    protected Pair Pair { get; private set; }

    public BaseRun(RunMode mode, Pair pair, Timeframe timeframe, StrategyModel strategyModel, ILogger? logger = null)
    {
        dataframeProvider = DataframeProviderFactory.GetExchangeDataframeProvider(Exchange.Kucoin, timeframe, pair);
        this.mode = mode;
        this.Pair = pair;
        this.StrategyModel = strategyModel;
        ExchangeInterface = GetExchange(mode, logger);
        Logger = logger;
        var cParams = new StrategyConstructorParameters()
        { Exchange = ExchangeInterface, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
        StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out strategy);
    }

    public async Task RunAsync(int preloadCandles = 0)
    {
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => Logger?.LogDebug("{p}", p.Description));
        try
        {
            await dataframeProvider.LoadAndPreloadCandles(preloadCandles, progress);
            strategy.OnStart();
            DataFrame? frame;
            Logger?.LogTrace("Awaiting frames...");
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
    protected virtual async void Heartbeat(DataFrame frame)
    {
        if (frame.IsClosed == true)
        {
            var model = new LiveProcessModel(
                Environment.ProcessId,
                mode.ToString(),
                StrategyModel.Name,
                Pair.ForKucoin(),
                await ExchangeInterface.GetTotalBalanceAsync(),
                await ExchangeInterface.GetOpenPositionsNumberAsync());

            var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);
            var path = Path.Join(Paths.Processes, model.Pid.ToString() + ".json");
            File.WriteAllText(path, serialized);
        }
    }

    private static IExchange GetExchange(RunMode mode, ILogger? logger = null)
    {
        return mode switch
        {
            RunMode.Foretest => ExchangeFactory.GetLocalTestExchange(1000, logger),
            RunMode.Live => null!,//TODO change
            _ => null!,
        };
    }
}