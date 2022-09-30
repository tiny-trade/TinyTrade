using HandierCli;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TinyTrade.Services;

public class CommandLineService : IHostedService
{
    private readonly IServiceProvider services;
    private readonly ILogger logger;

    public CommandLine Cli { get; private set; }

    public CommandLineService(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        this.services = services;
        logger = loggerFactory.CreateLogger("CLI");
        Cli = CommandLine.Factory().ExitOn("exit", "quit").OnUnrecognized(cmd => logger.LogError("{cmd} not recognized", cmd)).Build();
        RegisterCommands();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Cli.Run();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RegisterCommands()
    {
        Cli.Register(Command.Factory("help")
         .InhibitHelp()
         .Description("display the available commands")
         .ArgumentsHandler(ArgumentsHandler.Factory())
         .Add((handler) => Cli.Logger.LogInfo(Cli.Print())));
    }
}