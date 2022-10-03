namespace TinyTrade.Indicators;

public class Atr
{
    private readonly int period;
    private readonly Queue<float> tr;
    private float prevClose;
    private float atr;
    private int count;

    public Atr(int period = 14)
    {
        this.period = period;
        prevClose = 0;
        tr = new Queue<float>();
        atr = 0;
        count = 0;
    }

    public float? ComputeNext(float high, float low, float close)
    {
        count += 1;
        float lastElement = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
        tr.Enqueue(lastElement);
        if (tr.Count > period)
        {
            tr.Dequeue();
        }

        prevClose = close;

        if (count <= period + 1)
        {
            atr = tr.Average();
            return count == period + 1 ? (atr * (period - 1) + lastElement) / period : null;
        }
        else
        {
            atr = (atr * (period - 1) + lastElement) / period;
            return atr;
        }
    }
}