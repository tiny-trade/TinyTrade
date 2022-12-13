using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

/// <summary>
/// A condition that is perpetually true until the <see cref="callback"/> is true
/// </summary>
public class PerpetualCondition : AbstractCondition
{
    private readonly Predicate<DataFrame> callback;

    public PerpetualCondition(Predicate<DataFrame> callback)
    {
        this.callback = callback;
    }

    public override void Tick(DataFrame frame)
    {
        IsSatisfied = callback(frame);
    }
}