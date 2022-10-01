using HandierCli;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Globalization;
using TinyTrade.Core;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Models;
using TinyTrade.Core.Strategy;
using TinyTrade.Services.Data;
using TinyTrade.Statics;

namespace TinyTrade.Services;

internal class BacktestService
{
    private readonly ILogger logger;
    private readonly IStrategyResolver strategyResolver;
    private readonly IDataDownloadService downloadService;

    public BacktestService(ILoggerProvider provider, IStrategyResolver strategyResolver, IDataDownloadService downloadService)
    {
        logger = provider.CreateLogger(string.Empty);
        this.strategyResolver = strategyResolver;
        this.downloadService = downloadService;
    }

    public async Task RunBacktest(string pair, TimeInterval interval, string strategyFile)
    {
        var exchange = new TestExchange(logger, 100);
        var strategyModel = JsonConvert.DeserializeObject<StrategyModel>(File.ReadAllText(strategyFile));
        if (strategyModel is null)
        {
            logger.LogError("Unable to deserialize {s} file", strategyFile);
            return;
        }
        var cParams = new StrategyConstructorParameters(strategyModel.Parameters, strategyModel.Genotype, logger, exchange);
        if (!strategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy))
        {
            logger.LogError("Unable to instiantiate {s} strategy", strategyModel.Name);
            return;
        }
        logger.LogDebug("Resolved strategy {s}", strategy);
        var bar = ConsoleProgressBar.Factory().Lenght(20).Build();
        var progress = new Progress<(string, float)>(p => bar.Report(p.Item2, p.Item1));
        await downloadService.DownloadData(pair, interval, progress);
        bar.Dispose();
        var spinner = ConsoleSpinner.Factory().Info("Loading data ").Frames(12, "-   ", "--  ", "--- ", "----", " ---", "  --", "   -", "    ").Build();
        var frames = await spinner.Await(GetIntervalFrames(interval, pair, Timeframe.FlagToMinutes(strategyModel.Timeframe)));
        logger.LogTrace("{c} klines loaded", frames.Count);

        bar = ConsoleProgressBar.Factory().Lenght(20).Build();
        bar.Report(0F, $"Evaluating {strategy}");
        strategy.OnStart();
        for (var i = 0; i < frames.Count; i++)
        {
            strategy.UpdateState(frames[i]);
            bar.Report(i / (float)(frames.Count - 1));
        }
        strategy.OnStop();
        bar.Dispose();
        logger.LogInformation("Evaluation completed");
    }

    private async Task<List<DataFrame>> GetIntervalFrames(TimeInterval interval, string pair, int granularity)
    {
        var frames = new List<DataFrame>();
        await Task.Run(() =>
        {
            var module = 0;
            float moduleO = 0;
            ulong moduleOt = 0;
            float moduleH = 0;
            float moduleL = 0;
            float moduleV = 0;

            var prefix = $"{Paths.UserData}/{pair}-1m-";
            var periods = interval.GetPeriods();
            foreach (var p in periods)
            {
                var path = prefix + p + ".csv";
                if (!File.Exists(path)) continue;
                using var csvParser = new TextFieldParser(path);
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";", "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                while (!csvParser.EndOfData)
                {
                    var fields = csvParser.ReadFields();
                    if (fields is null || fields.Length < 6) continue;

                    var ot = ulong.Parse(fields[0], CultureInfo.InvariantCulture);
                    var o = float.Parse(fields[1], CultureInfo.InvariantCulture);
                    var h = float.Parse(fields[2], CultureInfo.InvariantCulture);
                    var l = float.Parse(fields[3], CultureInfo.InvariantCulture);
                    var c = float.Parse(fields[4], CultureInfo.InvariantCulture);
                    var v = float.Parse(fields[5], CultureInfo.InvariantCulture);
                    var ct = ulong.Parse(fields[6], CultureInfo.InvariantCulture);

                    if (module == 0)
                    {
                        moduleOt = ot;
                        moduleO = o;
                        moduleH = h;
                        moduleL = l;
                    }
                    module++;
                    moduleL = moduleL > l ? l : moduleL;
                    moduleH = moduleH < h ? h : moduleH;

                    if (module != 0 && module % granularity == 0)
                    {
                        var frame = new DataFrame(moduleOt, moduleO, moduleH, moduleL, c, moduleV, ct, true);
                        frames.Add(frame);
                        module = 0;
                    }
                    else
                    {
                        var frame = new DataFrame(ot, o, h, l, c, v, ct, false);
                        frames.Add(frame);
                    }
                }
            }
        });
        return frames;
    }
}