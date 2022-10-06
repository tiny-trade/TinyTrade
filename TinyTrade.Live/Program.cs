// args passed: "mode" "strategy file" "pair"

using HandierCli.CLI;
using Newtonsoft.Json;
using TinyTrade.Core.Models;
using TinyTrade.Statics;
using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();

// Create the args handler for matching arguments
args = new string[] { "foretest", "C:\\Users\\bltmt\\Documents\\Progetti\\TinyTrade\\bin\\Debug\\net6.0\\simple.json", "BNBUSDT" };
var handler = ArgumentsHandler.Factory()
                .Mandatory("mode", new string[] { "foretest", "live" })
                .Mandatory("strategy file", @".json$")
                .Mandatory("pair symbol", @"USDT$").Build();

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

return;