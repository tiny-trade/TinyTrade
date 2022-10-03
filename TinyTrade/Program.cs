using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Logging;
using TinyTrade.Services;
using TinyTrade.Services.Data;
using TinyTrade.Services.Hosted;

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
        services.AddSingleton<RunService>();
        services.AddSingleton<SnapService>();
    })
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddProvider(new CliLoggerProvider(cli));
    })
    .Build();

ConsoleExtensions.ClearConsoleLine();
Console.Title = "TinyTrade";
Console.WriteLine("==========  TinyTrade ==========\n");
await host.RunAsync();