using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Indicators;

namespace TinyTrade.Strategies;

public class AtrStochRsiEmaStrategy : AbstractStrategy
{
    private readonly Atr atr;
    private readonly Ema ema1;
    private readonly Ema ema2;
    private readonly Ema ema3;
    private readonly StochRsi stochRsi;
    private readonly float riskRewardRatio;
    private readonly float atrFactor;
    private readonly float stakePercentage;
    private readonly int intervalTolerance;
    private readonly ILogger? logger;
    private readonly int lowStochRsi;
    private readonly int highStochRsi;
    private float? lastStochK = null;
    private float? lastStochD = null;

    public AtrStochRsiEmaStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
        logger = parameters.Logger;
        riskRewardRatio = parameters.Traits.TraitValueOrDefault("riskRewardRatio", 1F);
        atrFactor = parameters.Traits.TraitValueOrDefault("atrFactor", 1F);
        stakePercentage = parameters.Traits.TraitValueOrDefault("stakePercentage", 0.1F);
        intervalTolerance = parameters.Traits.TraitValueOrDefault("intervalTolerance", 2);
        atr = new Atr();
        ema1 = new Ema(parameters.Traits.TraitValueOrDefault("ema1Period", 15));
        ema2 = new Ema(parameters.Traits.TraitValueOrDefault("ema2Period", 50));
        ema3 = new Ema(parameters.Traits.TraitValueOrDefault("ema3Period", 125));
        lowStochRsi = parameters.Traits.TraitValueOrDefault("lowStochRsi", 30);
        highStochRsi = parameters.Traits.TraitValueOrDefault("highStochRsi", 70);
        stochRsi = new StochRsi();
        InjectConditions();
    }

    protected override IEnumerable<Indicator> GetIndicators() => new Indicator[] { atr, ema1, ema2, ema3, stochRsi };

    protected override float GetMargin(DataFrame frame) => CachedTotalBalance is null ? 0F : (float)(stakePercentage * CachedTotalBalance);

    protected override float GetStopLoss(OrderSide side, DataFrame frame)
    {
        var ratio = (float)(atrFactor * atr.Last)!;
        return side switch
        {
            OrderSide.Buy => frame.Close - ratio,
            OrderSide.Sell => frame.Close + ratio,
            _ => frame.Close,
        };
    }

    protected override float GetTakeProfit(OrderSide side, DataFrame frame)
    {
        var ratio = (float)(atrFactor * atr.Last)!;
        return side switch
        {
            OrderSide.Buy => frame.Close + (ratio * riskRewardRatio),
            OrderSide.Sell => frame.Close - (ratio * riskRewardRatio),
            _ => frame.Close,
        };
    }

    protected override void ResetState()
    {
        lastStochK = null;
        lastStochD = null;
    }

    protected override Task Tick(DataFrame frame)
    {
        atr.ComputeNext(frame.High, frame.Low, frame.Close);
        ema1.ComputeNext(frame.Close);
        ema2.ComputeNext(frame.Close);
        ema3.ComputeNext(frame.Close);
        (lastStochK, lastStochD) = stochRsi.Last;
        stochRsi.ComputeNext(frame.Close);
        return Task.CompletedTask;
    }

    private void InjectConditions()
    {
        InjectLongConditions(
            new PerpetualCondition(f =>
                ema1.Last is not null && ema2.Last is not null && ema3.Last is not null &&
                ema3.Last < ema2.Last && ema2.Last < ema1.Last && ema1.Last < f.Close),
            new EventCondition(f =>
            {
                var stoch = stochRsi.Last;
                return stoch is not (null, null) && lastStochK is not null && lastStochD is not null &&
                lastStochK < lastStochD && stoch.Item1 > stoch.Item2 && stoch.Item2 < lowStochRsi;
            }, f =>
            {
                var stoch = stochRsi.Last;
                return stoch.Item1 < stoch.Item2 || stoch.Item2 > lowStochRsi;
            }, intervalTolerance));

        InjectShortConditions(
            new PerpetualCondition(f =>
                ema1.Last is not null && ema2.Last is not null && ema3.Last is not null &&
                ema3.Last > ema2.Last && ema2.Last > ema1.Last && ema1.Last > f.Close),
            new EventCondition(f =>
            {
                var stoch = stochRsi.Last;
                return stoch is not (null, null) && lastStochK is not null && lastStochD is not null &&
                lastStochK > lastStochD && stoch.Item1 < stoch.Item2 && stoch.Item2 > highStochRsi;
            }, f =>
            {
                var stoch = stochRsi.Last;
                return stoch.Item1 > stoch.Item2 || stoch.Item2 < highStochRsi;
            }, intervalTolerance));
    }
}