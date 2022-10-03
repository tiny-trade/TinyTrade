using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

public class EventCondition : Condition
{
    private readonly Predicate<DataFrame> callback;
    private readonly int tolerance;
    private int currentTolerance;

    public EventCondition(Predicate<DataFrame> callback, int tolerance = 1)
    {
        this.callback = callback;
        this.tolerance = tolerance;
        currentTolerance = 0;
    }

    public override void Tick(DataFrame frame)
    {
        if (!IsSatisfied && callback(frame))
        {
            IsSatisfied = true;
            currentTolerance = 0;
        }
        else
        {
            IsSatisfied = currentTolerance <= tolerance;
        }

        if (IsSatisfied)
        {
            currentTolerance++;
        }
    }

    public override void Reset()
    {
        base.Reset();
        currentTolerance = 0;
    }
}