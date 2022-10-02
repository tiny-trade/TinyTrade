namespace TinyTrade.Indicators;

public class Atr
{
    private int period;
    private float prevClose;
    private float atr;
    private int count;
    private Queue<float> queue;
    private float lastElement;

    public Atr(int period = 14)
    {
        this.period = period;
        queue = new Queue<float>();
        atr = 0;
        count = 0;
        lastElement = 0;
    }

    public float? ComputeNext(float high, float low, float close)
    {
        count += 1;
        lastElement = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
        queue.Enqueue(lastElement);
        prevClose = close;

        if (queue.Count > period)
        {
            queue.Dequeue();
        }

        if (count <= period + 1)
        {
            atr = queue.Average();
            if (count == period + 1)
            {
                return (atr * (period - 1) + lastElement) / period;
            }
            else return null;
        }
        else
        {
            atr = (atr * (period - 1) + lastElement) / period;
            return atr;
        }
    }
}