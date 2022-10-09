using Newtonsoft.Json;

namespace TinyTrade.Core.Constructs;

[Serializable]
public class StrategyTrait
{
    [JsonProperty("key")]
    public string Key { get; private set; } = null!;

    [JsonProperty("value")]
    public float? Value { get; private set; } = null;

    public StrategyTrait(string key, float value)
    {
        Key = key;
        Value = value;
    }
}