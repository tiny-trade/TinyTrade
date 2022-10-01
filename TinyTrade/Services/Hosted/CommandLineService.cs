using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("Running in {env}\n", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        return cli.RunAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RegisterCommands()
    {
        cli.Register(Command.Factory("help")
         .InhibitHelp()
         .Description("display the available commands")
         .ArgumentsHandler(ArgumentsHandler.Factory())
         .Add((handler) => logger.LogInformation("{p}", cli.Print())));

        cli.Register(Command.Factory("backtest")
            .Description("run a simulation on historical market data")
            .ArgumentsHandler(ArgumentsHandler.Factory().Positional("strategy file").Positional("pair symbol").Flag("/d", "download data if not present"))
            .Add(handler =>
            {
                logger.LogDebug("Simulating backtesting");
                var service = services.GetRequiredService<BacktestService>();
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
            .ArgumentsHandler(ArgumentsHandler.Factory().Positional("strategy file").Positional("pair symbol").Flag("/d", "download data if not present"))
            .Add(handler =>
            {
                logger.LogDebug("Simulating snapping");
                var service = services.GetRequiredService<SnapService>();
            }));
    }
}