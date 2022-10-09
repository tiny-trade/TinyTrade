using HandierCli.Progress;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Exchanges.Backtest;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Services;

internal class BacktestService
{
    private readonly ILogger logger;

    public BacktestService(ILoggerProvider provider)
    {
        logger = provider.CreateLogger(string.Empty);
    }

    public async Task<BacktestResultModel?> RunParallelBacktest(ParallelBacktestDataframeProvider provider, Guid strategyIdentifier, StrategyModel strategyModel)
    {
        try
        {
            var exchange = new LocalTestExchange(100, logger);
            var cParams = new StrategyConstructorParameters()
            { Exchange = exchange, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
            if (!StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy)) return null;

            provider.Reset(strategyIdentifier);
            exchange.Reset();

            strategy.OnStart();
            DataFrame? frame;
            while ((frame = await provider.Next(strategyIdentifier)) is not null)
            {
                await strategy.UpdateState(frame);
            }
            strategy.OnStop();
            var result = new BacktestResultModel(
                                exchange.ClosedPositions,
                                provider.Timeframe,
                                exchange.InitialBalance,
                                exchange.GetTotalBalance(),
                                provider.FramesCount);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e.Message);
            return null;
        }
    }

    public async Task<BacktestResultModel?> RunBacktest(Pair pair, TimeInterval interval, StrategyModel strategyModel)
    {
        float initialBalance = 100;
        var exchange = ExchangeFactory.GetLocalTestExchange(initialBalance, logger);
        var provider = DataframeProviderFactory.GetBacktestDataframeProvider(interval, pair, Timeframe.FromFlag(strategyModel.Timeframe));
        var bar = ConsoleProgressBar.Factory().Lenght(20).Build();
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => bar.Report(p.Progress, p.Description));
        await provider.Load(progress);
        bar.Dispose();

        var cParams = new StrategyConstructorParameters()
        { Exchange = exchange, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
        if (!StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy)) return null;

        var watch = new Stopwatch();
        watch.Start();
        strategy.OnStart();
        var spinner = ConsoleSpinner.Factory().Info("Evaluating ").Frames(12, "-   ", "--  ", "--- ", "----", " ---", "  --", "   -", "    ").Build();

        await spinner.Await(Task.Run(async () =>
        {
            DataFrame? frame;
            while ((frame = await provider.Next()) is not null)
            {
                await strategy.UpdateState(frame);
            }
        }));
        strategy.OnStop();
        watch.Stop();

        var millis = watch.ElapsedMilliseconds;
        var result = new BacktestResultModel(
                exchange.ClosedPositions,
                provider.Timeframe,
                exchange.InitialBalance,
                exchange.GetTotalBalance(),
                provider.FramesCount);
        logger.LogTrace("Processed {c} klines in just {ms}ms O.O - Hail to the C#!", provider.FramesCount, millis);
        logger.LogInformation("Evaluation result:\n{r}", JsonConvert.SerializeObject(result, Formatting.Indented));
        return result;
    }
}