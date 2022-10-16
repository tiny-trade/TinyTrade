namespace TinyTrade.Indicators;

public class Atr : Indicator
{
    private readonly int period;
    private readonly Queue<float> queue;
    private float prevClose;
    private float atr;
    private int count;

    public float? Last { get; private set; } = null;

    public Atr(int period = 14)
    {
        this.period = period;
        prevClose = 0;
        queue = new Queue<float>();
        atr = 0;
        count = 0;
    }

    public override void Reset()
    {
        atr = 0;
        prevClose = 0;
        count = 0;
        Last = null;
        queue.Clear();
    }

    public float? ComputeNext(float high, float low, float close)
    {
        count += 1;
        var lastElement = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
        queue.Enqueue(lastElement);
        if (queue.Count > period)
        {
            queue.Dequeue();
        }

        prevClose = close;

        if (count <= period + 1)
        {
            atr = queue.Average();
            float? res = count == period + 1 ? (atr * (period - 1) + lastElement) / period : null;
            Last = res;
            return res;
        }
        else
        {
            atr = (atr * (period - 1) + lastElement) / period;
            Last = atr;
            return atr;
        }
    }
}