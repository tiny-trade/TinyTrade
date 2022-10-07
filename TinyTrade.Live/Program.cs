// args passed: "mode" "strategy file" "pair"

using HandierCli.CLI;
using HandierCli.Log;
using Kucoin.Net.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges.Backtest;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Statics;
using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();

// Create the args handler for matching arguments
var handler = ArgumentsHandler.Factory()
                .Mandatory("mode", new string[] { "foretest", "live" })
                .Mandatory("strategy file", @".json$")
                .Mandatory("pair symbol", @"USDT$").Build();

var logger = new AdvancedLogger();

// Load args into the handler and verify their correctness
handler.LoadArgs(args);
var res = handler.Fits();
if (!res.Successful)
{
    // Possibility to print the errors using res: FitResult, since process is in background, no need for now
    Environment.Exit(1);
}

// Get args
var mode = handler.GetPositional(0);
var strategyFile = handler.GetPositional(1);
var pair = handler.GetPositional(2);

// Handle errors
if (!File.Exists(strategyFile))
{
    Environment.Exit(1);
}
var strategyModel = JsonConvert.DeserializeObject<StrategyModel>(File.ReadAllText(strategyFile));
if (strategyModel is null)
{
    Environment.Exit(1);
}

// Arguments are successful
Directory.CreateDirectory(Paths.Processes);

var model = new LiveProcessModel(Environment.ProcessId, mode, strategyFile, pair);
var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);
var path = Path.Join(Paths.Processes, model.Pid.ToString() + ".json");
File.WriteAllText(path, serialized);

if (mode == "foretest")
{
    float initialBalance = 100;
    var exchange = new BacktestExchange(initialBalance, logger);
    var cParams = new StrategyConstructorParameters(strategyModel.Parameters, strategyModel.Genotype, logger, exchange);
    if (!StrategyResolver.TryResolveStrategy(strategyModel.Name, cParams, out var strategy))
    {
        logger.Log(LogLevel.Error, "Unable to instiantiate {s} strategy", strategyModel.Name);
        return;
    }
    logger.Log(LogLevel.Debug, "Resolved strategy {s}", strategy);
    var provider = new KucoinDataframeProvider(pair, new Timeframe(strategyModel.Timeframe));
    await provider.Load();
    strategy.OnStart();
    DataFrame? frame;
    while ((frame = await provider.Next()) is not null)
    {
        await strategy.UpdateState((DataFrame)frame);
        if (frame.IsClosed == true)
        {
            logger.Log(LogLevel.Information, "total balance :{b}", exchange.GetTotalBalance());
        }
    }
    strategy.OnStop();
}

return;