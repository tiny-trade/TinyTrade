using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Models;

[Serializable]
public class StrategyModel
{
    public string Strategy { get; init; } = null!;

    public string Timeframe { get; init; } = null!;

    public Dictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    public List<Trait> Traits { get; init; } = new List<Trait>();
}