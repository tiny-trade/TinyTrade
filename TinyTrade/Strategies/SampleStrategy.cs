﻿using TinyTrade.Core;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Strategies;

internal class SampleStrategy : AbstractStrategy
{
    public SampleStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
    }

    protected override float GetStakeAmount() => 0F;

    protected override float GetStopLoss(IExchange.Side side, DataFrame frame) => 0F;

    protected override float GetTakeProfit(IExchange.Side side, DataFrame frame) => 0F;

    protected override void Tick(DataFrame frame)
    {
        base.Tick(frame);
    }
}