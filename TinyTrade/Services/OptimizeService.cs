using GeneticSharp;
using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Models;
using TinyTrade.Opt.Genes;
using TinyTrade.Opt.Modules;

namespace TinyTrade.Services;

internal class OptimizeService
{
    private readonly ILogger logger;
    private readonly BacktestService backtestService;

    public OptimizeService(BacktestService backtestService, ILoggerProvider loggerProvider)
    {
        logger = loggerProvider.CreateLogger(string.Empty);
        this.backtestService = backtestService;
    }

    public async Task Optimize(Pair pair, TimeInterval interval, OptimizableStrategyModel strategyModel)
    {
        try
        {
            var minValues = new double[strategyModel.Genes.Count];
            var maxValues = new double[strategyModel.Genes.Count];
            var bits = new int[strategyModel.Genes.Count];
            var floatingDigits = new int[strategyModel.Genes.Count];

            for (int i = 0; i < strategyModel.Genes.Count; i++)
            {
                var current = strategyModel.Genes[i];
                minValues[i] = current.Min is null ? double.NegativeInfinity : (double)current.Min;
                maxValues[i] = current.Max is null ? double.NegativeInfinity : (double)current.Max;
                floatingDigits[i] = current.Type is GeneType.Float ? 3 : 0;
                // TODO change this
                var stringRepr = Convert.ToString(Convert.ToInt64(maxValues[i] * Math.Pow(10.0, floatingDigits[i])), 2);
                bits[i] = stringRepr.Length;
            }

            var chromosome = new FloatingPointChromosome(minValues, maxValues, bits, floatingDigits);
            var fitness = new StrategyFitness(backtestService, pair, interval, strategyModel, logger);
            await fitness.Load();
            var selection = new TournamentSelection(6, true);
            var pop = new Population(58, 128, chromosome);
            var crossover = new UniformCrossover(0.5F);
            var mutation = new UniformMutation();
            var termination = new GenerationNumberTermination(200);

            var ga = new GeneticAlgorithm(pop, fitness, selection, crossover, mutation)
            { Termination = termination };

            var latestFitness = 0.0;
            var taskExecutor = new ParallelTaskExecutor { MinThreads = 4, MaxThreads = 128 };
            //ga.TaskExecutor = taskExecutor;

            ga.GenerationRan += (sender, e) =>
            {
                var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                var bestFitness = bestChromosome!.Fitness!.Value;

                if (bestFitness != latestFitness)
                {
                    latestFitness = bestFitness; var phenotype = bestChromosome.ToFloatingPoints();

                    Console.WriteLine(
                     "Generation {0}: ({1},{2},{3},{4}) = {5}",
                     ga.GenerationsNumber,
                     phenotype[0],
                     phenotype[1],
                     phenotype[2],
                     phenotype[3],
                     bestFitness
                 );
                }
            };
            await Task.Run(() => ga.Start());
            //ga.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}