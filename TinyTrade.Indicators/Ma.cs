namespace TinyTrade.Indicators;

public class Ma
{
    private readonly int period;
    private readonly Queue<float> firstValues;

    public float? Last { get; private set; } = null;

    public Ma(int period = 20)
    {
        this.period = period;
        firstValues = new Queue<float>();
    }

    public float? ComputeNext(float close)
    {
        firstValues.Enqueue(close);
        if (firstValues.Count > period)
        {
            firstValues.Dequeue();
        }

        float? res = firstValues.Count < period ? null : firstValues.Average();
        Last = res;
        return res;
    }

    public void Reset()
    {
        Last = null;
        firstValues.Clear();
    }
}