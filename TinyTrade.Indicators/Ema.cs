namespace TinyTrade.Indicators;

public class Ema : Indicator
{
    private readonly int period;
    private readonly int smoothing;
    private readonly Queue<float> firstValues;
    private float previousValue;

    public float? Last { get; private set; } = null;

    public Ema(int period = 20, int smoothing = 2)
    {
        firstValues = new Queue<float>();
        this.period = period;
        this.smoothing = smoothing;
        previousValue = 0;
    }

    public float? ComputeNext(float close)
    {
        if (firstValues.Count < period)
        {
            firstValues.Enqueue(close);
            previousValue = firstValues.Average();
            return null;
        }
        else
        {
            var smooth = smoothing / (1F + period);
            previousValue = close * smooth + previousValue * (1F - smooth);
            Last = previousValue;
            return previousValue;
        }
    }

    public override void Reset()
    {
        Last = null;
        previousValue = 0;
        firstValues.Clear();
    }
}