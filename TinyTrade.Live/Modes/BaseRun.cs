using HandierCli.CLI;
using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Exchanges.Offline;
using TinyTrade.Core.Models;
using TinyTrade.Core.Shared;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Live.Communication;
using TinyTrade.Live.Models;
using TinyTrade.Statics;

namespace TinyTrade.Live.Modes;

internal class BaseRun
{
    private readonly IExchangeDataframeProvider dataframeProvider;

    private readonly IStrategy strategy;
    private readonly RunMode mode;

    private IpcHandler? ipcHandler;

    protected StrategyModel StrategyModel { get; private set; }

    protected IExchange ExchangeInterface { get; private set; }

    protected ILogger? Logger { get; private set; }

    protected Pair Pair { get; private set; }

    public BaseRun(RunMode mode, Exchange exchange, Pair pair, Timeframe timeframe, StrategyModel strategyModel, ILogger? logger = null)
    {
        dataframeProvider = DataframeProviderFactory.GetExchangeDataframeProvider(exchange, timeframe, pair);
        this.mode = mode;
        Pair = pair;
        StrategyModel = strategyModel;
        ExchangeInterface = GetExchange(mode, logger);
        Logger = logger;
        StrategyResolver.TryResolveStrategy(
            strategyModel.Strategy,
            new StrategyConstructorParameters()
            {
                Exchange = ExchangeInterface,
                Logger = logger,
                Parameters = strategyModel.Parameters,
                Traits = strategyModel.Traits
            }, out strategy);
    }

    public async Task RunAsync(int preloadCandles = 0)
    {
        ipcHandler = new IpcHandler(GetPipeCommands());
        ipcHandler.Open();
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => Logger?.LogDebug("{p}", p.Description));
        try
        {
            await dataframeProvider.LoadAndPreloadCandles(preloadCandles, progress);
            DataFrame? frame;
            Logger?.LogTrace("Awaiting frames...");
            while ((frame = await dataframeProvider.Next()) is not null)
            {
                await strategy.UpdateState(frame);
                Heartbeat(frame);
            }
        }
        catch (Exception e)
        {
            Logger?.LogError("Exception captured: {e}", e);
        }
        finally { ipcHandler.Close(); }
    }

    /// <summary>
    ///   Called when a kline is received, after updating the strategy
    /// </summary>
    protected virtual async void Heartbeat(DataFrame frame)
    {
        if (/*frame.IsClosed == */true)
        {
            var withdrawn = ExchangeInterface is OfflineExchange offlineExchange ? offlineExchange.WithdrawedBalance : 0;
            var model = new LiveProcessModel(
                Environment.ProcessId,
                mode,
                StrategyModel.Strategy,
                Pair.ForKucoin(),
                (float)withdrawn,
                (float)await ExchangeInterface.GetTotalBalanceAsync(),
                await ExchangeInterface.GetOpenPositionsNumberAsync());

            Directory.CreateDirectory(Paths.Processes);
            var serialized = SerializationHandler.Serialize(model);
            var path = Path.Join(Paths.Processes, model.Pid.ToString() + ".json");
            File.WriteAllText(path, serialized);
            Console.WriteLine($"Serialized model {serialized}");
        }
    }

    private static IExchange GetExchange(RunMode mode, ILogger? logger = null)
    {
        return mode switch
        {
            RunMode.Foretest => ExchangeFactory.GetLocalTestExchange(1000, logger),
            // TODO change !
            RunMode.Live => null!,
            _ => null!,
        };
    }

    private IEnumerable<Command> GetPipeCommands()
    {
        return new List<Command>()
        {
            Command.Factory("withdraw").WithArguments(
                ArgumentsHandler.Factory().Mandatory("amount", "[0-9]+"))
            .AddAsync(async(handler) =>
            {
                if(!float.TryParse(handler.GetPositional(0), out var amount))return;
                await ExchangeInterface.WithdrawFromTradingBalanceAsync(amount);
            })
        };
    }
}