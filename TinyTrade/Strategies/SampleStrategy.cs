using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Strategies;

public class SampleStrategy : AbstractStrategy
{
    public SampleStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
    }

    protected override float GetStakeAmount() => 0F;

    protected override float GetStopLoss(OrderSide side, DataFrame frame) => 0F;

    protected override float GetTakeProfit(OrderSide side, DataFrame frame) => 0F;

    protected override void Tick(DataFrame frame)
    {
        base.Tick(frame);
    }
}