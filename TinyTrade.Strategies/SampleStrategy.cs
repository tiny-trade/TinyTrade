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
    private float? atrVal = null;
    private float? ema1Val = null;
    private float? ema2Val = null;
    private float? ema3Val = null;
    private float? stochK = null;
    private float? lastStochK = null;
    private float? stochD = null;
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
            ema1Val is not null && ema2Val is not null && ema3Val is not null && ema3Val < ema2Val && ema2Val < ema1Val && ema1Val < f.Close));
        AddLongCondition(new EventCondition(f =>
            lastStochK is not null && stochK is not null && lastStochD is not null && stochD is not null && lastStochK <= lastStochD && stochK > stochD, intervalTolerance));

        AddShortCondition(new PerpetualCondition(f =>
            ema1Val is not null && ema2Val is not null && ema3Val is not null && ema3Val > ema2Val && ema2Val > ema1Val && ema1Val > f.Close));
        AddShortCondition(new EventCondition(f =>
            lastStochK is not null && stochK is not null && lastStochD is not null && stochD is not null && lastStochK >= lastStochD && stochK < stochD, intervalTolerance));
    }

    protected override float GetStakeAmount() => stakePercentage;

    protected override float GetStopLoss(OrderSide side, DataFrame frame)
    {
        var ratio = (float)(atrFactor * atrVal)!;
        return side switch
        {
            OrderSide.Buy => frame.Close - ratio,
            OrderSide.Sell => frame.Close + ratio,
            _ => frame.Close,
        };
    }

    protected override float GetTakeProfit(OrderSide side, DataFrame frame)
    {
        var ratio = (float)(atrFactor * atrVal)!;
        return side switch
        {
            OrderSide.Buy => frame.Close + ratio * riskRewardRatio,
            OrderSide.Sell => frame.Close - ratio * riskRewardRatio,
            _ => frame.Close,
        };
    }

    protected override void Tick(DataFrame frame)
    {
        base.Tick(frame);

        atrVal = atr.ComputeNext(frame.High, frame.Low, frame.Close);
        ema1Val = ema1.ComputeNext(frame.Close);
        ema2Val = ema2.ComputeNext(frame.Close);
        ema3Val = ema3.ComputeNext(frame.Close);
        lastStochK = stochK;
        lastStochD = stochD;
        (stochK, stochD) = stochRsi.ComputeNext(frame.Close);
        logger.LogTrace("{price} | {atr}, {e1}, {e2}, {e3}, ({k},{d})", frame.Close, atrVal, ema1Val, ema2Val, ema3Val, stochK, stochD);
    }
}