using HandierCli.Log;
using System.Diagnostics;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Models;
using TinyTrade.Core.Shared;
using TinyTrade.Core.Statics;
using TinyTrade.Live.Modes;
using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();
// Create the args handler for matching arguments
var handler = ArgumentsHandlerFactory.ForRun().Build();

var logger = new AdvancedLogger();

// Load args into the handler and verify their correctness
handler.LoadArgs(args);
var res = handler.Fits();
if (!res.Successful)
{
    // Possibility to print the errors using res: FitResult, since process is in background, no need for now
    Console.WriteLine(res.Reason);
    foreach (var fail in res.FailedFits)
    {
        Console.WriteLine($"wrong value {fail.Item2} for argument {fail.Item1}");
    }
    Environment.Exit(1);
}

// Get args
var mode = handler.GetPositional(0);
var strategyFile = handler.GetPositional(1);
var pair = handler.GetPositional(2);
var exchangeStr = handler.GetPositional(3);

// Handle errors
if (!File.Exists(strategyFile))
{
    Environment.Exit(1);
}
var strategyModel = SerializationHandler.Deserialize<StrategyModel>(File.ReadAllText(strategyFile));
if (strategyModel is null)
{
    Environment.Exit(1);
}
try
{
    Process.GetCurrentProcess().Exited += OnProcessExit;
    var exchange = Enum.Parse<Exchange>(exchangeStr, true);
    var runMode = Enum.Parse<RunMode>(mode, true);
    var run = new BaseRun(runMode, exchange, Pair.Parse(pair), Timeframe.FromFlag(strategyModel.Timeframe), strategyModel, logger);
    var trait = strategyModel.Traits.MaxBy(p => p.Value);
    await run.RunAsync(trait is null ? 0 : (int)trait.Value!);
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.ReadLine();
}

void OnProcessExit(object? sender, EventArgs e)
{
}