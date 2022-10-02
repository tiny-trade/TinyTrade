namespace TinyTrade.Core.Strategy;

internal class PerpetualCondition : Condition
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