using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

/// <summary>
/// Condition used to detect a specific event that remains true for the specified number of klines: <see cref="tolerance"/>
/// </summary>
public class EventCondition : AbstractCondition
{
    private readonly Predicate<DataFrame> callback;
    private readonly Predicate<DataFrame>? resetCondition;
    private readonly int tolerance;
    private int currentTolerance;

    public EventCondition(Predicate<DataFrame> callback, Predicate<DataFrame>? resetCondition = null, int tolerance = 1)
    {
        this.callback = callback;
        this.resetCondition = resetCondition;
        this.tolerance = tolerance;
        currentTolerance = 0;
    }

    public override void Tick(DataFrame frame)
    {
        var status = callback(frame);
        if (status)
        {
            if (!IsSatisfied)
            {
                IsSatisfied = true;
                currentTolerance = 0;
            }
        }
        else if (IsSatisfied)
        {
            IsSatisfied = currentTolerance <= tolerance;
        }

        if (IsSatisfied)
        {
            currentTolerance++;
            if (resetCondition is not null && resetCondition(frame))
            {
                Reset();
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        currentTolerance = 0;
    }
}