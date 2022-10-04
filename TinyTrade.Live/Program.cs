// args passed: "mode" "strategy file" "pair"

using Newtonsoft.Json;
using System.Diagnostics;
using TinyTrade.Core.Models;
using TinyTrade.Statics;
using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();

Directory.CreateDirectory(Paths.Processes);
var pid = Process.GetCurrentProcess().Id;
var model = new LiveProcessModel(pid, args[0], args[1], args[2]);
var file = JsonConvert.SerializeObject(model);
System.IO.File.WriteAllText(Paths.Processes + "/" + pid + ".json", file);

return;