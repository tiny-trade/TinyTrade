using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;
using TinyTrade.Indicators;

namespace TinyTrade.Strategies;

public class SampleStrategy : AbstractStrategy
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
    private readonly ILogger logger;
    private float? lastStochK = null;
    private float? lastStochD = null;

    public SampleStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
        logger = parameters.Logger;
        riskRewardRatio = parameters.Genotype.GetValueOrDefault("riskRewardRatio");
        atrFactor = parameters.Genotype.GetValueOrDefault("atrFactor");
        stakePercentage = parameters.Genotype.GetValueOrDefault("stakePercentage");
        intervalTolerance = (int)parameters.Genotype.GetValueOrDefault("intervalTolerance", 1);
        atr = new Atr();
        ema1 = new Ema((int)parameters.Genotype.GetValueOrDefault("ema1Period"));
        ema2 = new Ema((int)parameters.Genotype.GetValueOrDefault("ema2Period"));
        ema3 = new Ema((int)parameters.Genotype.GetValueOrDefault("ema3Period"));
        stochRsi = new StochRsi();

        AddLongCondition(new PerpetualCondition(f =>
            ema1.Last is not null && ema2.Last is not null && ema3.Last is not null && ema3.Last < ema2.Last && ema2.Last < ema1.Last && ema1.Last < f.Close));
        AddLongCondition(new EventCondition(f =>
        {
            var stoch = stochRsi.Last;
            return stoch is not (null, null) && lastStochK is not null && lastStochD is not null && lastStochK <= lastStochD && stoch.Item1 > stoch.Item2;
        }, intervalTolerance));

        AddShortCondition(new PerpetualCondition(f =>
            ema1.Last is not null && ema2.Last is not null && ema3.Last is not null && ema3.Last > ema2.Last && ema2.Last > ema1.Last && ema1.Last > f.Close));
        AddShortCondition(new EventCondition(f =>
        {
            var stoch = stochRsi.Last;
            return stoch is not (null, null) && lastStochK is not null && lastStochD is not null && lastStochK >= lastStochD && stoch.Item1 < stoch.Item2;
        }, intervalTolerance));
    }

    protected override float GetStakeAmount() => stakePercentage;

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

    protected override void Tick(DataFrame frame)
    {
        base.Tick(frame);

        atr.ComputeNext(frame.High, frame.Low, frame.Close);
        ema1.ComputeNext(frame.Close);
        ema2.ComputeNext(frame.Close);
        ema3.ComputeNext(frame.Close);
        (lastStochK, lastStochD) = stochRsi.Last;
        stochRsi.ComputeNext(frame.Close);
    }
}