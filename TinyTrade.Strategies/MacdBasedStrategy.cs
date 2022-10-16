using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Statics;
using TinyTrade.Core.Strategy;
using TinyTrade.Indicators;

namespace TinyTrade.Strategies;

public class MacdBasedStrategy : AbstractStrategy
{
    private readonly Macd macd;
    private readonly Ema ema;
    private readonly float stakePercentage;
    private readonly float riskRewardRatio;
    private readonly float stopLossRatio;
    private readonly int intervalTolerance;
    private float? lastHist;

    public MacdBasedStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
        var fastPeriod = parameters.Traits.TraitValueOrDefault("fastPeriod", 12);
        var slowPeriod = parameters.Traits.TraitValueOrDefault("slowPeriod", 26);
        var signalPeriod = parameters.Traits.TraitValueOrDefault("signalPeriod", 9);
        var emaPeriod = parameters.Traits.TraitValueOrDefault("emaPeriod", 200);
        intervalTolerance = parameters.Traits.TraitValueOrDefault("intervalTolerance", 2);
        stopLossRatio = parameters.Traits.TraitValueOrDefault("stopLossRatio", 1.1F);
        riskRewardRatio = parameters.Traits.TraitValueOrDefault("riskRewardRatio", 1.5F);
        stakePercentage = parameters.Traits.TraitValueOrDefault("stakePercentage", 0.1F);
        macd = new Macd(fastPeriod, slowPeriod, signalPeriod);
        ema = new Ema(emaPeriod);
        InjectConditions();
    }

    protected override Task Tick(DataFrame frame)
    {
        ema.ComputeNext(frame.Close);
        (lastHist, _, _) = macd.Last;
        macd.ComputeNext(frame.Close);
        return Task.CompletedTask;
    }

    protected override float GetMargin(DataFrame frame) => CachedTotalBalance is null ? 0F : (float)(stakePercentage * CachedTotalBalance);

    protected override float GetStopLoss(OrderSide side, DataFrame frame)
    {
        var emaV = (float)ema.Last!;
        var diff = Math.Abs(frame.Close - emaV);
        return side switch
        {
            OrderSide.Buy => frame.Close - (diff * stopLossRatio),
            OrderSide.Sell => frame.Close + (diff * stopLossRatio),
            _ => frame.Close,
        };
    }

    protected override float GetTakeProfit(OrderSide side, DataFrame frame)
    {
        var emaV = (float)ema.Last!;
        var diff = Math.Abs(frame.Close - emaV);
        return side switch
        {
            OrderSide.Buy => frame.Close + (diff * stopLossRatio * riskRewardRatio),
            OrderSide.Sell => frame.Close - (diff * stopLossRatio * riskRewardRatio),
            _ => frame.Close,
        };
    }

    protected override void ResetState()
    {
        lastHist = null;
    }

    protected override IEnumerable<Indicator> GetIndicators() => new Indicator[] { macd, ema };

    private void InjectConditions()
    {
        InjectLongConditions(
            new PerpetualCondition(c => ema.Last is not null && ema.Last < c.Close),
            new EventCondition(c =>
                ema.Last is not null &&
                macd.Last is not (null, null, null) &&
                lastHist is not null &&
                // Cross up below the zero line
                lastHist < 0 && macd.Last.Item1 > 0 && macd.Last.Item2 < 0 && macd.Last.Item3 < 0,
                f => macd.Last.Item2 > 0 && macd.Last.Item3 > 0,
                intervalTolerance));

        InjectShortConditions(
            new PerpetualCondition(c => ema.Last is not null && ema.Last > c.Close),
            new EventCondition(c =>
                ema.Last is not null &&
                macd.Last is not (null, null, null) &&
                lastHist is not null &&
                // Cross down above the zero line
                lastHist > 0 && macd.Last.Item1 < 0 && macd.Last.Item2 > 0 && macd.Last.Item3 > 0,
                f => macd.Last.Item2 < 0 && macd.Last.Item3 < 0,
                intervalTolerance));
    }
}