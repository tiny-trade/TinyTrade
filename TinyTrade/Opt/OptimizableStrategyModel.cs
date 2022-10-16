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
    public string Strategy { get; init; } = null!;

    public string Timeframe { get; init; } = null!;

    public Dictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    public List<StrategyGene> Traits { get; init; } = new List<StrategyGene>();

    public static implicit operator StrategyModel(OptimizableStrategyModel optModel)
        => new StrategyModel()
        {
            Strategy = optModel.Strategy,
            Parameters = optModel.Parameters,
            Timeframe = optModel.Timeframe,
            Traits = optModel.Traits.ConvertAll(g => g as Trait)
        };
}