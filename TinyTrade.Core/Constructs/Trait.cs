using Newtonsoft.Json;

namespace TinyTrade.Core.Constructs;

/// <summary>
///   Base class representing a generic trait
/// </summary>
[Serializable]
public class Trait
{
    [JsonProperty("key")]
    public string Key { get; private set; } = null!;

    [JsonProperty("value")]
    public float? Value { get; private set; } = null;

    public Trait(string key, float value)
    {
        Key = key;
        Value = value;
    }
}