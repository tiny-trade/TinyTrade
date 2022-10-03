namespace TinyTrade.Indicators;

public class StochRsi
{
    private int period;
    private int fastkPeriod;
    private int slowdPeriod;
    private Queue<float?> values;
    private Rsi rsi;
    private Stoch stoch;

    public StochRsi(int period = 14, int fastkPeriod = 13, int slowdPeriod = 3)
    {
        this.period = period;
        this.fastkPeriod = fastkPeriod;
        this.slowdPeriod = slowdPeriod;

        values = new Queue<float?>();
        rsi = new Rsi(period);
        stoch = new Stoch(period, 3, fastkPeriod);
    }

    public (float?, float?) ComputeNext(float close)
    {
        float? minVal;
        float? maxVal;
        float? diff;
        float? fastK;
        float? fastD;

        var current = rsi.ComputeNext(close);
        values.Enqueue(current);
        if (values.Count > period) values.Dequeue();

        minVal = values.Min();
        maxVal = values.Max();
        diff = (maxVal - minVal);
        if (diff == 0) diff = 1;

        fastK = (current - minVal) / diff * 100F;
        fastD = stoch.ComputeNext(current, current, current).Item1;

        return fastD == null ? (null, null) : ((float?, float?))(fastK, fastD);
    }
}