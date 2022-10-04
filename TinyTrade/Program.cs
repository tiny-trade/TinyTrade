using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Services;
using TinyTrade.Services.Data;
using TinyTrade.Services.Hosted;
using TinyTrade.Services.Logging;
using TinyTrade.Strategies.Link;

Console.Title = "TinyTrade";
Console.WriteLine("==========  TinyTrade ==========\n");
var cli = CommandLine.Factory()
        .OnUnrecognized((logger, cmd) => logger.LogError("{cmd} not recognized", cmd))
        .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

        services.AddHostedService<CommandLineService>();

        services.AddSingleton(provider => cli);
        services.AddSingleton<IDataDownloadService, BinanceDataDownloadService>();
        services.AddTransient<BacktestService>();
        services.AddSingleton<LiveService>();
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