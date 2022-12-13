using HandierCli.CLI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Services;
using TinyTrade.Services.Hosted;
using TinyTrade.Services.Logging;
using TinyTrade.Strategies.Link;

Console.Title = "TinyTrade";
Console.WriteLine("==========  TinyTrade ==========\n");
var cli = CommandLine.Factory()
        .OnUnrecognized((logger, cmd) => logger.Log($"{cmd} not recognized", ConsoleColor.DarkRed))
        .RegisterHelpCommand()
        .GlobalHelpSymbol("-h")
        .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

        services.AddHostedService<CommandLineHostedService>();
        services.AddHostedService<CleanupHostedService>();

        services.AddSingleton(provider => cli);
        services.AddTransient<BacktestService>();
        services.AddTransient<OptimizeService>();
        services.AddSingleton<IpcService>();
        services.AddSingleton<RunService>();
        services.AddSingleton<SnapService>();
    })
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddProvider(new CliLoggerProvider(cli));
    })
    .Build();

var loggerProvider = host.Services.GetRequiredService<ILoggerProvider>();
var logger = loggerProvider.CreateLogger(string.Empty);

// DO NOT REMOVE THIS, necessary for preventing Visual Studio from stripping assemblies that are used solely through reflection
TinyTradeStrategiesAssembly.DummyLink();

logger.LogDebug("Strategies assembly linked");

await host.RunAsync();