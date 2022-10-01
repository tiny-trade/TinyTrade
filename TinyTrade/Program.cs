using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyTrade.Logging;
using TinyTrade.Services;
using TinyTrade.Services.DataDownload;
using TinyTrade.Services.Hosted;

Console.Title = "TinyTrade";
Console.WriteLine("==========  TinyTrade ==========\n");

var cli = CommandLine.Factory()
        .OnUnrecognized((logger, cmd) => logger.LogError("{cmd} not recognized", cmd))
        .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<CommandLineService>();

        services.AddSingleton(provider => cli);
        services.AddSingleton<IStrategyResolver, StrategyResolver>();
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

var b = new BinanceDataDownloadService(new CliLoggerProvider(cli));
await b.DownloadData("BNBUSDT", "2022-02|2022-07", "user_data/data");

await host.RunAsync();