using Newtonsoft.Json;

namespace TinyTrade.Core.Models;

[Serializable]
public class StrategyModel
{
    [JsonProperty("strategy")]
    public string Name { get; init; } = null!;

    [JsonProperty("timeframe")]
    public string Timeframe { get; init; } = null!;

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    [JsonProperty("genotype")]
    public Dictionary<string, float> Genotype { get; init; } = new Dictionary<string, float>();
}