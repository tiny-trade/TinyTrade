using HandierCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TinyTrade.Services;

Console.Title = "TinyTrade";
Logger.ConsoleInstance.LogInfo("----- NFT GENERATOR -----\n\n", ConsoleColor.DarkCyan);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
        services.AddHostedService<CommandLineService>())
    .Build();

await host.RunAsync();