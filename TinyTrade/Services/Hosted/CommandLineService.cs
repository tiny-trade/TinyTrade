using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Strategies.Link;

namespace TinyTrade.Services.Hosted;

internal class CommandLineService : IHostedService
{
    private readonly ILogger logger;
    private readonly CommandLine cli;
    private readonly IServiceProvider services;

    public CommandLineService(IServiceProvider services, ILoggerProvider provider)
    {
        cli = services.GetRequiredService<CommandLine>();
        logger = provider.CreateLogger(string.Empty);
        RegisterCommands();
        this.services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        AssemblyLink.DummyLink();
        logger.LogDebug("Strategies assembly linked");
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
        cli.Register(Command.Factory("help")
         .InhibitHelp()
         .Description("display the available commands")
         .ArgumentsHandler(ArgumentsHandler.Factory())
         .Add((handler) => logger.LogInformation("{p}", cli.Print())));

        cli.Register(Command.Factory("backtest")
            .Description("run a simulation on historical market data")
            .ArgumentsHandler(ArgumentsHandler.Factory().Positional("strategy file").Positional("interval pattern").Positional("pair symbol"))
            .AddAsync(async handler =>
            {
                var service = services.GetRequiredService<BacktestService>();
                var intervalPattern = handler.GetPositional(1);
                var pair = handler.GetPositional(2);
                var strategyFile = handler.GetPositional(0);
                await service.RunBacktest(pair, intervalPattern, strategyFile);
            }));

        cli.Register(Command.Factory("run")
            .Description("run a foretest simulation or a live trading session")
            .ArgumentsHandler(ArgumentsHandler.Factory().Positional("strategy file").Positional("pair symbol").Flag("/d", "download data if not present"))
            .Add(handler =>
            {
                logger.LogDebug("Simulating running");
                var service = services.GetRequiredService<RunService>();
            }));

        cli.Register(Command.Factory("snap")
            .Description("look for active foretest simulations or live sessions currently running")
            .ArgumentsHandler(ArgumentsHandler.Factory())
            .Add(handler =>
            {
                logger.LogDebug("Simulating snapping");
                var service = services.GetRequiredService<SnapService>();
                service.Snapshot();
            }));
    }
}