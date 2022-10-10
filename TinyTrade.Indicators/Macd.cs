namespace TinyTrade.Indicators;

public class Macd
{
    private readonly Ema emaFast;
    private readonly Ema emaSlow;
    private readonly Ema emaSignal;

    public (float?, float?, float?) Last { get; private set; } = (null, null, null);

    public Macd(int fast = 12, int slow = 26, int signal = 9)
    {
        emaFast = new Ema(fast);
        emaSlow = new Ema(slow);
        emaSignal = new Ema(signal);
    }

    public void Reset()
    {
        Last = (null, null, null);
        emaFast.Reset();
        emaSlow.Reset();
        emaSignal.Reset();
    }

    /// <summary>
    /// </summary>
    /// <param name="close"> </param>
    /// <returns> (hist, fast, slow) </returns>
    public (float?, float?, float?) ComputeNext(float close)
    {
        var emaF = emaFast.ComputeNext(close);
        var emaS = emaSlow.ComputeNext(close);

        if (emaF is null || emaS is null)
        {
            return (null, null, null);
        }
        var macd = emaF - emaS;

        var sig = emaSignal.ComputeNext((float)macd);
        var res = (sig is null ? null : macd - sig, macd, sig);
        Last = res;
        return res;
    }
}