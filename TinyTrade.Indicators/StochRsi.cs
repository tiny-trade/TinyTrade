namespace TinyTrade.Indicators;

public class StochRsi
{
    private readonly Rsi rsi;
    private readonly Stoch stoch;
    private readonly Ma dMa;

    public (float?, float?) Last { get; private set; } = (null, null);

    public StochRsi(int period = 14, int fastkPeriod = 3, int slowdPeriod = 3)
    {
        rsi = new Rsi(period);
        stoch = new Stoch(period, 3, fastkPeriod);
        dMa = new Ma(slowdPeriod);
    }

    public void Reset()
    {
        Last = (null, null);
        rsi.Reset();
        stoch.Reset();
        dMa.Reset();
    }

    public (float?, float?) ComputeNext(float close)
    {
        float? fastD;
        var current = rsi.ComputeNext(close);
        fastD = current is null ? null : stoch.ComputeNext((float)current, (float)current, (float)current).Item1;
        if (fastD is not null)
        {
            var slowD = dMa.ComputeNext((float)fastD);
            var res = slowD is not null ? ((float?, float?))(fastD, slowD) : (null, null);
            Last = res;
            return res;
        }
        else
        {
            return (null, null);
        }
    }
}