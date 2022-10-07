using HandierCli.Progress;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges.Backtest;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Services.Data;

namespace TinyTrade.Services;

internal class BacktestService
{
    private readonly ILogger logger;
    private readonly IDataDownloadService downloadService;

    public BacktestService(ILoggerProvider provider, IDataDownloadService downloadService)
    {
        logger = provider.CreateLogger(string.Empty);
        this.downloadService = downloadService;
    }

    public async Task RunBacktest(string pair, TimeInterval interval, string strategyFile)
    {
        float initialBalance = 100;
        var exchange = new BacktestExchange(initialBalance, logger);
        var strategyModel = JsonConvert.DeserializeObject<StrategyModel>(File.ReadAllText(strategyFile));
        if (strategyModel is null)
        {
            logger.LogError("Unable to deserialize {s} file", strategyFile);
            return;
        }
        var cParams = new StrategyConstructorParameters(strategyModel.Parameters, strategyModel.Genotype, logger, exchange);
        if (!StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy))
        {
            logger.LogError("Unable to instiantiate {s} strategy", strategyModel.Name);
            return;
        }
        logger.LogDebug("Resolved strategy {s}", strategy);
        var provider = new BacktestDataframeProvider(interval, pair, Timeframe.FlagToMinutes(strategyModel.Timeframe));
        var bar = ConsoleProgressBar.Factory().Lenght(20).Build();
        var progress = new Progress<(string, float)>(p => bar.Report(p.Item2, p.Item1));
        await downloadService.DownloadData(pair, interval, progress);
        bar.Dispose();
        var spinner = ConsoleSpinner.Factory().Info("Loading data ").Frames(12, "-   ", "--  ", "--- ", "----", " ---", "  --", "   -", "    ");
        await spinner.Build().Await(provider.Load());

        spinner.Info("Evaluating ");

        var watch = new Stopwatch();
        watch.Start();
        strategy.OnStart();
        await spinner.Build().Await(Task.Run(async () =>
        {
            DataFrame? frame;
            while ((frame = await provider.Next()) is not null)
            {
                await strategy.UpdateState((DataFrame)frame);
            }
        }));
        strategy.OnStop();
        bar.Dispose();
        watch.Stop();

        var millis = watch.ElapsedMilliseconds;
        if (exchange is BacktestExchange testExchange)
        {
            var result = new BacktestResultModel(
                testExchange.ClosedPositions,
                new Timeframe(strategyModel.Timeframe),
                initialBalance,
                testExchange.GetTotalBalance(),
                provider.FramesCount);
            logger.LogTrace("Processed {c} klines in just {ms}ms O.O - Hail to the C#!", provider.FramesCount, millis);
            logger.LogInformation("Evaluation result:\n{r}", JsonConvert.SerializeObject(result, Formatting.Indented));
        }
        else
        {
            logger.LogWarning("Internal error: unable to retrieve test exchange for computing evaluation results");
        }
        logger.LogInformation("Evaluation completed");
    }
}