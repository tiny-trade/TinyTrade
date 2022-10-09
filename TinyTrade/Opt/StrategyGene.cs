using Newtonsoft.Json;
using System.ComponentModel;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Opt;

public enum GeneType
{ Integer, Float }

/// <summary>
///   Decorator model of <see cref="Trait"/> in order to assign boundaries and gene types
/// </summary>
internal class StrategyGene : Trait
{
    [JsonProperty("min")]
    public float? Min { get; private set; } = null;

    [JsonProperty("max")]
    public float? Max { get; private set; } = null;

    [DefaultValue(GeneType.Float)]
    [JsonProperty(PropertyName = "type", DefaultValueHandling = DefaultValueHandling.Populate)]
    public GeneType Type { get; private set; }

    public StrategyGene(string key, float value, (float min, float max) bounds, GeneType type) : base(key, value)
    {
        Min = bounds.min;
        Max = bounds.max;
        Type = type;
    }
}