// args passed: "mode" "strategy file" "pair"

using HandierCli.CLI;
using HandierCli.Log;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Models;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Live.Modes;
using TinyTrade.Statics;
using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();
// Create the args handler for matching arguments
var handler = ArgumentsHandler.Factory()
                .Mandatory("mode", new string[] { "foretest", "live" })
                .Mandatory("strategy file", @".json$")
                .Mandatory("pair symbol", @"[A-Z]+-USDT$").Build();

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
try
{
    var runMode = Enum.Parse<RunMode>(mode, true);
    var run = new BaseRun(runMode, Pair.Parse(pair), Timeframe.FromFlag(strategyModel.Timeframe), strategyModel, logger);
    var trait = strategyModel.Traits.MaxBy(p => p.Value);
    await run.RunAsync(trait is null ? 0 : (int)trait.Value!);
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.ReadLine();
}