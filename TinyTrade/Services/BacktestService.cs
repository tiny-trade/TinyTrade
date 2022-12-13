using HandierCli.Progress;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Services;

/// <summary>
/// Service handling backtesting
/// </summary>
internal class BacktestService
{
    private readonly ILogger logger;

    public BacktestService(ILoggerProvider provider)
    {
        logger = provider.CreateLogger(string.Empty);
    }

    /// <summary>
    ///   Run a backtest that is compatible with parallel running using an existing <see cref="ParallelBacktestDataframeProvider"/> and a
    ///   <see cref="Guid"/> of the strategy
    /// </summary>
    /// <param name="provider"> </param>
    /// <param name="strategyIdentifier"> </param>
    /// <param name="strategyModel"> </param>
    /// <returns> </returns>
    public async Task<List<BacktestResultModel>?> RunParallelBacktest(ParallelBacktestDataframeProvider provider, Guid strategyIdentifier, StrategyModel strategyModel)
    {
        try
        {
            var exchange = ExchangeFactory.GetLocalTestExchange(100, logger);
            var cParams = new StrategyConstructorParameters()
            { Exchange = exchange, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
            if (!StrategyResolver.TryResolveStrategy(strategyModel.Strategy, cParams, out var strategy)) return null;

            exchange.Reset();
            provider.Reset(strategyIdentifier);

            var results = new List<BacktestResultModel>();
            var watch = new Stopwatch();
            while (provider.HasAnotherBatch(strategyIdentifier))
            {
                watch.Restart();
                DataFrame? frame;
                while ((frame = await provider.Next(strategyIdentifier)) is not null)
                {
                    await strategy.UpdateState(frame);
                }
                watch.Stop();
                var result = new BacktestResultModel(
                                    exchange.ClosedPositions,
                                    provider.Timeframe,
                                    exchange.WithdrawedBalance,
                                    exchange.InitialBalance,
                                    exchange.GetTotalBalance(),
                                    exchange.TotalFees,
                                    provider.FramesCount,
                                    watch.ElapsedMilliseconds);
                strategy.Reset();
                results.Add(result);
            }
            provider.Clear(strategyIdentifier);
            return results;
        }
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e.Message);
            return null;
        }
    }

    public async Task<BacktestResultModel?> RunBacktest(Pair pair, TimeInterval interval, StrategyModel strategyModel)
    {
        var bar = ConsoleProgressBar.Factory().Lenght(50).Build();
        try
        {
            float initialBalance = 1000;
            var exchange = ExchangeFactory.GetLocalTestExchange(initialBalance, logger);
            var provider = DataframeProviderFactory.GetBacktestDataframeProvider(interval, pair, Timeframe.FromFlag(strategyModel.Timeframe));
            var progress = new Progress<IDataframeProvider.LoadProgress>(p => bar.Report(p.Progress, p.Description));
            await provider.Load(progress);
            bar.Dispose();

            var cParams = new StrategyConstructorParameters()
            { Exchange = exchange, Logger = logger, Parameters = strategyModel.Parameters, Traits = strategyModel.Traits };
            if (!StrategyResolver.TryResolveStrategy(strategyModel.Strategy, cParams, out var strategy)) return null;

            var watch = new Stopwatch();
            watch.Start();
            var spinner = ConsoleSpinner.Factory().Info("Evaluating ").Frames(12, "-   ", "--  ", "--- ", "----", " ---", "  --", "   -", "    ").Build();

            await spinner.Await(Task.Run(async () =>
            {
                DataFrame? frame;
                while ((frame = await provider.Next()) is not null)
                {
                    await strategy.UpdateState(frame);
                }
            }));
            watch.Stop();

            return new BacktestResultModel(
                    exchange.ClosedPositions,
                    provider.Timeframe,
                    exchange.WithdrawedBalance,
                    exchange.InitialBalance,
                    exchange.GetTotalBalance(),
                    exchange.TotalFees,
                    provider.FramesCount,
                    watch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            bar.Dispose();
            logger.LogError("Exception captured: {e}", e.Message);
            return null;
        }
    }
}