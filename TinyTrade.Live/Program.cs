// args passed: "mode" "strategy file" "pair"

using TinyTrade.Strategies.Link;

TinyTradeStrategiesAssembly.DummyLink();

Console.WriteLine(args);
await Task.Delay(10000);
Console.WriteLine("Dying");
await Task.Delay(1000);
return;