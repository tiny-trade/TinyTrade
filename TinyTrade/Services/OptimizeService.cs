using GeneticSharp;
using Microsoft.Extensions.Logging;
using System.Text;
using TinyTrade.Core.Constructs;
using TinyTrade.Opt;
using TinyTrade.Statics;

namespace TinyTrade.Services;

internal class OptimizeService
{
    private readonly ILogger logger;
    private readonly BacktestService backtestService;
    private GeneticAlgorithm? ga;
    private OptimizableStrategyModel? templateModel;
    private string? reportPath;
    private double lastFitness = 0;

    public OptimizeService(BacktestService backtestService, ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
        this.backtestService = backtestService;
    }

    public async Task Optimize(Pair pair, TimeInterval interval, OptimizableStrategyModel strategyModel)
    {
        try
        {
            reportPath = $"{Paths.GeneticReports}/{pair.ForBinance()}_{interval}_{strategyModel.Name}.txt";
            templateModel = strategyModel;
            var minValues = new double[strategyModel.Genes.Count];
            var maxValues = new double[strategyModel.Genes.Count];
            var bits = new int[strategyModel.Genes.Count];
            var fractionDigits = new int[strategyModel.Genes.Count];
            var values = new List<double?>();
            for (int i = 0; i < strategyModel.Genes.Count; i++)
            {
                var current = strategyModel.Genes[i];
                minValues[i] = current.Min is null ? double.NegativeInfinity : (double)current.Min;
                maxValues[i] = current.Max is null ? double.PositiveInfinity : (double)current.Max;
                fractionDigits[i] = current.Type is GeneType.Float ? 2 : 0;
                var stringRepr = Convert.ToString(Convert.ToInt64(maxValues[i] * Math.Pow(10.0, fractionDigits[i])), 2);
                bits[i] = stringRepr.Length;
                values.Add(current.Value);
            }

            var chromosome =
                new IdFloatingPointChromosome(
                    Guid.NewGuid(),
                    minValues, maxValues,
                    bits, fractionDigits,
                    values.Any(v => v is null) ? null : values.ConvertAll(v => (double)v!).ToArray());

            var fitness = new StrategyFitnessEvaluator(backtestService, pair, interval, strategyModel, logger);
            await fitness.Load();

            var selection = new RankSelection();
            var population = new Population(128, 192, chromosome);
            var crossover = new UniformCrossover();// new VotingRecombinationCrossover(8, 4);
            var mutation = new UniformMutation(true);
            var termination = new FitnessStagnationTermination(128);// new GenerationNumberTermination(200);
            var taskExecutor = new ParallelTaskExecutor { MinThreads = 4, MaxThreads = 32 };
            ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = termination,
                TaskExecutor = taskExecutor,
                MutationProbability = 0.15F
            };
            ga.GenerationRan += GenerationReport;
            logger.LogInformation("Generation {g} running...", 0);
            ga.Start();
            logger.LogInformation("Optimization completed");
        }
        catch (Exception e)
        {
            logger.LogError("Exception captured: {e}", e);
        }
    }

    private void GenerationReport(object? sender, EventArgs args)
    {
        if (templateModel is null || ga is null || string.IsNullOrEmpty(reportPath)) return;
        if (ga.BestChromosome is not IdFloatingPointChromosome bestChromosome) return;
        var bestFitness = bestChromosome!.Fitness!.Value;
        if (lastFitness < bestFitness)
        {
            lastFitness = bestFitness;
            var phenotype = bestChromosome.ToFloatingPoints();
            var zip = phenotype.Zip(templateModel.Genes);
            var stringBuilder = new StringBuilder($"Generation {ga.GenerationsNumber}:\n");
            foreach (var (value, gene) in zip)
            {
                stringBuilder.Append("| ");
                stringBuilder.Append(gene.Key);
                stringBuilder.Append(" = ");
                stringBuilder.Append(value);
                stringBuilder.Append('\n');
            }
            stringBuilder.Append("F = ");
            stringBuilder.Append(bestFitness);
            logger.LogDebug("New population best \n{b}\n===> {p}", stringBuilder.ToString(), reportPath);
            File.WriteAllText(reportPath, stringBuilder.ToString());
        }
        logger.LogInformation("\n{d}\nGeneration {g} running...", new string('-', 50), ga.GenerationsNumber + 1);
    }
}