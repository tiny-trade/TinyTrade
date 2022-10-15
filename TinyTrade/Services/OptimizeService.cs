using GeneticSharp;
using Microsoft.Extensions.Logging;
using System.Text;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Statics;
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
    private string? modelPath;
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
            reportPath = $"{Paths.GeneticReports}/{pair.ForBinance()}_{interval}_{strategyModel.Strategy}.txt";
            modelPath = $"{Paths.GeneticReports}/{pair.ForBinance()}_{interval}_{strategyModel.Strategy}.json";
            templateModel = strategyModel;
            var minValues = new double[strategyModel.Traits.Count];
            var maxValues = new double[strategyModel.Traits.Count];
            var bits = new int[strategyModel.Traits.Count];
            var fractionDigits = new int[strategyModel.Traits.Count];
            var values = new List<double?>();
            for (int i = 0; i < strategyModel.Traits.Count; i++)
            {
                var current = strategyModel.Traits[i];
                minValues[i] = current.Min is null ? double.NegativeInfinity : (double)current.Min;
                maxValues[i] = current.Max is null ? double.PositiveInfinity : (double)current.Max;
                fractionDigits[i] = current.Type is GeneType.Float ? 2 : 0;
                var stringRepr = Convert.ToString(Convert.ToInt64(maxValues[i] * Math.Pow(10.0, fractionDigits[i])), 2);
                bits[i] = stringRepr.Length;
                values.Add(current.Value);
            }

            IdFloatingPointChromosome? chromosome =
                new IdFloatingPointChromosome(
                    Guid.NewGuid(),
                    minValues, maxValues,
                    bits, fractionDigits,
                    values.Any(v => v is null) ? null : values.ConvertAll(v => (double)v!).ToArray());

            var fitness = new StrategyFitnessEvaluator(backtestService, pair, interval, strategyModel, logger);
            await fitness.Load();

            var selection = new RankSelection();
            var population = new Population(96, 128, chromosome);
            var crossover = new UniformCrossover();// new VotingRecombinationCrossover(8, 4);
            var mutation = new UniformMutation(true);
            var termination = new FitnessStagnationTermination(16);// new GenerationNumberTermination(200);
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
        if (templateModel is null || ga is null || string.IsNullOrEmpty(reportPath) || string.IsNullOrEmpty(modelPath)) return;
        if (ga.BestChromosome is not IdFloatingPointChromosome bestChromosome) return;
        var bestFitness = bestChromosome!.Fitness!.Value;
        if (lastFitness < bestFitness)
        {
            lastFitness = bestFitness;
            var phenotype = bestChromosome.ToFloatingPoints();
            var zip = phenotype.Zip(templateModel.Traits);
            var stringBuilder = new StringBuilder($"Generation {ga.GenerationsNumber}:\n");
            var bestGenes = new List<StrategyGene>();
            foreach (var (value, gene) in zip)
            {
                stringBuilder.Append("| ");
                stringBuilder.Append(gene.Key);
                stringBuilder.Append(" = ");
                stringBuilder.Append(value);
                stringBuilder.Append('\n');
                bestGenes.Add(new StrategyGene(gene.Key, (float)value, gene.Min, gene.Max, gene.Type));
            }
            stringBuilder.Append("F = ");
            stringBuilder.Append(bestFitness);
            var bestModel = new OptimizableStrategyModel()
            {
                Strategy = templateModel.Strategy,
                Parameters = templateModel.Parameters,
                Timeframe = templateModel.Timeframe,
                Traits = bestGenes
            };
            logger.LogDebug("New population best \n{b}\n===> {p}", stringBuilder.ToString(), reportPath);
            File.WriteAllText(reportPath, stringBuilder.ToString());
            File.WriteAllText(modelPath, SerializationHandler.Serialize(bestModel));
        }
        logger.LogInformation("\n{d}\nGeneration {g} running...", new string('-', 50), ga.GenerationsNumber + 1);
    }
}