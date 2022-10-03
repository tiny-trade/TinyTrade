namespace TinyTrade.Indicators;

public class Ma
{
    private readonly int period;
    private readonly Queue<float> firstValues;

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

        return firstValues.Count < period ? null : firstValues.Average();
    }

    public void Reset() => firstValues.Clear();
}