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

    public async Task<BacktestResultModel?> RunCachedBacktest(BacktestDataframeProvider provider, LocalTestExchange exchange, StrategyModel strategyModel, bool verbose = true)
    {
        var cParams = new StrategyConstructorParameters()
        { Exchange = exchange, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
        if (!StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy)) return null;

        provider.Reset();
        exchange.Reset();
        var watch = new Stopwatch();
        watch.Start();
        strategy.OnStart();
        if (verbose)
        {
            var spinner = ConsoleSpinner.Factory().Info("Evaluating ").Frames(12, "-   ", "--  ", "--- ", "----", " ---", "  --", "   -", "    ").Build();

            await spinner.Await(Task.Run(async () =>
            {
                DataFrame? frame;
                while ((frame = await provider.Next()) is not null)
                {
                    await strategy.UpdateState(frame);
                }
            }));
        }
        else
        {
            DataFrame? frame;
            while ((frame = await provider.Next()) is not null)
            {
                await strategy.UpdateState(frame);
            }
        }
        strategy.OnStop();
        watch.Stop();

        var millis = watch.ElapsedMilliseconds;
        if (exchange is LocalTestExchange testExchange)
        {
            var result = new BacktestResultModel(
                testExchange.ClosedPositions,
                provider.Timeframe,
                exchange.InitialBalance,
                testExchange.GetTotalBalance(),
                provider.FramesCount);
            if (verbose)
            {
                logger.LogTrace("Processed {c} klines in just {ms}ms O.O - Hail to the C#!", provider.FramesCount, millis);
                logger.LogInformation("Evaluation result:\n{r}", JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            return result;
        }
        else
        {
            logger.LogWarning("Internal error: unable to retrieve test exchange for computing evaluation results");
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

        return await RunCachedBacktest(provider, exchange, strategyModel);
    }
}