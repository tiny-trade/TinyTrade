using GeneticSharp;
using HandierCli.Progress;
using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Models;
using TinyTrade.Services;

namespace TinyTrade.Opt;

/// <summary>
///   Evaluator to evaluate <see cref="Core.Strategy.IStrategy"/> and assign fitnesses values
/// </summary>
internal class StrategyFitnessEvaluator : IFitness
{
    private readonly ILogger? logger;
    private readonly BacktestService backtestService;
    private readonly OptimizableStrategyModel templateModel;
    private readonly ParallelBacktestDataframeProvider provider;

    public StrategyFitnessEvaluator(BacktestService backtestService, Pair pair, TimeInterval interval, OptimizableStrategyModel strategyModel, ILogger? logger = null)
    {
        provider = DataframeProviderFactory.GetParallelBacktestDataframeProvider(interval, pair, Timeframe.FromFlag(strategyModel.Timeframe), interval.GetPeriods().Count());
        templateModel = strategyModel;
        this.backtestService = backtestService;
        this.logger = logger;
    }

    public async Task Load()
    {
        var bar = ConsoleProgressBar.Factory().Lenght(50).Build();
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => bar.Report(p.Progress, p.Description));
        await provider.Load(progress);
        bar.Dispose();
    }

    public double Evaluate(IChromosome chromosome)
    {
        if (chromosome is not IdFloatingPointChromosome strategyChromosome) return 0;
        var floats = strategyChromosome.ToFloatingPoints();
        if (floats.Length != templateModel.Traits.Count) return 0;
        var zip = templateModel.Traits.Zip(floats).ToList();
        var evaluationModel = new StrategyModel()
        {
            Strategy = templateModel.Strategy,
            Parameters = templateModel.Parameters,
            Timeframe = templateModel.Timeframe,
            Traits = zip.ConvertAll(z => new Trait(z.First.Key, (float)z.Second))
        };

        var res = backtestService.RunParallelBacktest(provider, strategyChromosome.Id, evaluationModel).Result;
        return res is null ? 0 : CalculateFitness(res);
    }

    private static double CalculateFitness(List<BacktestResultModel> resultModels)
    {
        double totalFitness = 0;
        foreach (var resultModel in resultModels)
        {
            var a = 0.65F;
            var g = 4.75F;
            var d = 1.75F;
            var c = 2.5F;
            var r = resultModel.FinalBalance / resultModel.InitialBalance;
            var wr = resultModel.WinRate;
            var r_penalize = 1F;
            var wr_penalize = 1F;
            if (r < 1) r_penalize = (float)Math.Pow(r, g);
            if (wr < 0.5F) wr_penalize = (float)Math.Pow(wr + 0.5F, d);
            var rFac = r_penalize * Math.Pow(r + 1, a);
            var wrFac = wr_penalize * Math.Pow(wr + 1, 1 - a);
            var feeFac = Math.Pow(resultModel.TotalFees + 1, c);
            var res = rFac + wrFac - feeFac;
            totalFitness += 365 * (res / resultModel.Days);
        }
        return totalFitness;
    }
}