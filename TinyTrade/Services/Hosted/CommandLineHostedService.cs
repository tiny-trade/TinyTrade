using HandierCli.CLI;
using HandierCli.Progress;
using HandierCli.Statics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Models;
using TinyTrade.Core.Shared;
using TinyTrade.Core.Statics;
using TinyTrade.Opt;

namespace TinyTrade.Services.Hosted;

internal class CommandLineHostedService : IHostedService
{
    private readonly ILogger logger;
    private readonly CommandLine cli;
    private readonly IServiceProvider services;

    public CommandLineHostedService(IServiceProvider services, ILoggerProvider provider)
    {
        cli = services.GetRequiredService<CommandLine>();
        logger = provider.CreateLogger(string.Empty);
        RegisterCommands();
        this.services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => cli.RunAsync(), cancellationToken);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ConsoleExtensions.ClearConsoleLine();
        var spinner = ConsoleSpinner.Factory().Info("See ya ").Frames(6, ":)", ":|", ":(", ":(", ":(", ":(").Completed("").Build();
        await spinner.Await(Task.Delay(750, cancellationToken));
    }

    private void RegisterCommands()
    {
        cli.Register(Command.Factory("backtest")
            .Description("run a simulation on historical market data")
            .WithArguments(ArgumentsHandlerFactory.ForBacktest())
            .AddAsync(async handler =>
            {
                var service = services.GetRequiredService<BacktestService>();
                var strategyFile = handler.GetPositional(0);
                var intervalPattern = handler.GetPositional(1);
                var pair = handler.GetPositional(2);
                var strategyModel = SerializationHandler.Deserialize<OptimizableStrategyModel>(File.ReadAllText(strategyFile));
                if (strategyModel is null)
                {
                    logger.LogError("Unable to deserialize {s} file", strategyFile);
                    return;
                }
                var result = await service.RunBacktest(Pair.Parse(pair), intervalPattern, strategyModel);
                if (result is null)
                {
                    logger.LogError("Result is null, something went really bad X(");
                }
                else
                {
                    var model = (BacktestResultModel)result;
                    logger.LogTrace("Processed {c} klines in just {ms}ms O.O - Hail to the C#!", model.Frames, model.ElapsedMillis);
                    logger.LogInformation("Evaluation result:\n{r}", SerializationHandler.Serialize(model));
                }
            }));

        cli.Register(Command.Factory("run")
            .Description("run a foretest simulation or a live trading session")
            .WithArguments(ArgumentsHandlerFactory.ForRun())
            .AddAsync(async handler =>
            {
                logger.LogDebug("Simulating running");
                var service = services.GetRequiredService<RunService>();
                var runMode = Enum.Parse<RunMode>(handler.GetPositional(0), true);
                var strategyFile = handler.GetPositional(1);
                var pair = handler.GetPositional(2);
                var exchange = Enum.Parse<Exchange>(handler.GetPositional(3), true);
                await service.RunLive(runMode, exchange, strategyFile, pair);
            }));

        cli.Register(Command.Factory("snap")
            .Description("look for active foretest simulations or live sessions currently running")
            .WithArguments(ArgumentsHandlerFactory.ForSnap())
            .Add(handler =>
            {
                logger.LogDebug("Simulating snapping");
                var service = services.GetRequiredService<SnapService>();
                service.Snapshot();
            }));

        cli.Register(Command.Factory("optimize")
           .Description("optimize strategies")
           .WithArguments(ArgumentsHandlerFactory.ForOptimize())
           .AddAsync(async handler =>
           {
               var service = services.GetRequiredService<OptimizeService>();
               var strategyFile = handler.GetPositional(0);
               var intervalPattern = handler.GetPositional(1);
               var pair = handler.GetPositional(2);
               var strategyModel = SerializationHandler.Deserialize<OptimizableStrategyModel>(File.ReadAllText(strategyFile));
               if (strategyModel is null)
               {
                   logger.LogError("Unable to deserialize {s} file", strategyFile);
                   return;
               }
               await service.Optimize(Pair.Parse(pair), intervalPattern, strategyModel);
           }));
    }
}