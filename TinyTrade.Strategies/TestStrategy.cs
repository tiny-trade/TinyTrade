using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;
using TinyTrade.Indicators;

namespace TinyTrade.Strategies;

public class TestStrategy : AbstractStrategy
{
    private readonly Atr atr;
    private readonly BollingerBands bBand;
    private readonly Ema ema;
    private readonly Ma ma;
    private readonly Rsi rsi;
    private readonly Stoch stoch;
    private readonly StochRsi stochRsi;
    private float? atrValue;
    private (float?, float?, float?) bollingerValue;
    private float? emaValue;
    private float? maValue;
    private float? rsiValue;
    private Macd macd;
    private (float?, float?) stochValue;
    private (float?, float?) stochRsiValue;

    public TestStrategy(StrategyConstructorParameters parameters) : base(parameters)
    {
        atr = new Atr();
        bBand = new BollingerBands(32, 4);
        ema = new Ema(50);
        ma = new Ma(50);
        rsi = new Rsi();
        stoch = new Stoch();
        stochRsi = new StochRsi();
        macd = new Macd(12, 26, 9);
    }

    protected override float GetStakeAmount() => 0F;

    protected override float GetStopLoss(OrderSide side, DataFrame frame) => 0F;

    protected override float GetTakeProfit(OrderSide side, DataFrame frame) => 0F;

    protected override void Tick(DataFrame frame)
    {
        base.Tick(frame);
        atrValue = atr.ComputeNext(frame.High, frame.Low, frame.Close);
        bollingerValue = bBand.ComputeNext(frame.High, frame.Low, frame.Close);
        emaValue = ema.ComputeNext(frame.Close);
        maValue = ma.ComputeNext(frame.Close);
        rsiValue = rsi.ComputeNext(frame.Close);
        stochValue = stoch.ComputeNext(frame.Close, frame.Low, frame.High);
        stochRsiValue = stochRsi.ComputeNext(frame.Close);
        var m = macd.ComputeNext(frame.Close);
        // backtest test.json 2021-01|2021-02 BNBUSDT
        //backtest test.json 2022-08|2022-09 BNBUSDT
        if (frame.CloseTime >= 1664580039999)
        {
            Logger.LogInformation("openTime: {ot} | close: {c} | atr: {atr} | bBand: {bband} | ema: {ema} | ma: {ma} | rsi: {rsi} | stoch: {stoch} | stochRsi: {stochRsi} | macd: {macd}", frame.OpenTime, frame.Close, atrValue, bollingerValue, emaValue, maValue, rsiValue, stochValue, stochRsiValue, m);
        }
    }
}