using Newtonsoft.Json;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Opt;

public enum GeneType
{ Integer, Float }

/// <summary>
///   Decorator model of <see cref="Trait"/> in order to assign boundaries and gene types
/// </summary>
public class StrategyGene : Trait
{
    public float? Min { get; private set; }

    public float? Max { get; private set; }

    public GeneType Type { get; private set; }

    [JsonConstructor]
    public StrategyGene(string key, float? value, float? min, float? max, GeneType type) : base(key, value)
    {
        Min = min;
        Max = max;
        Type = type;
    }

    public StrategyGene(string key, float? value, (float min, float max) bounds, GeneType type) : base(key, value)
    {
        Min = bounds.min;
        Max = bounds.max;
        Type = type;
    }
}