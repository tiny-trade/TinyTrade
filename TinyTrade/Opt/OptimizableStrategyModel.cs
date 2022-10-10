using Newtonsoft.Json;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Models;

namespace TinyTrade.Opt;

/// <summary>
///   Model that can be interchanged with <see cref="StrategyModel"/>, representing a strategy model with <see cref="StrategyGene"/> instead
///   of simple <see cref="Trait"/>
/// </summary>
[Serializable]
internal class OptimizableStrategyModel
{
    [JsonProperty("strategy")]
    public string Name { get; init; } = null!;

    [JsonProperty("timeframe")]
    public string Timeframe { get; init; } = null!;

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    [JsonProperty("traits")]
    public List<StrategyGene> Genes { get; init; } = new List<StrategyGene>();

    public static implicit operator StrategyModel(OptimizableStrategyModel optModel)
        => new StrategyModel()
        {
            Name = optModel.Name,
            Parameters = optModel.Parameters,
            Timeframe = optModel.Timeframe,
            Traits = optModel.Genes.ConvertAll(g => g as Trait)
        };
}