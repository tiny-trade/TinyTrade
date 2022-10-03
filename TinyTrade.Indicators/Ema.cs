namespace TinyTrade.Indicators;

public class Ema
{
    private readonly int period;
    private readonly int smoothing;
    private readonly List<float> firstValues;
    private float previousValue;
    private int length;

    public Ema(int period = 20, int smoothing = 2)
    {
        firstValues = new List<float>();
        this.period = period;
        this.smoothing = smoothing;
        previousValue = 0;
        length = 0;
    }

    public float? ComputeNext(float close)
    {
        if (length < period)
        {
            firstValues.Add(close);
            length++;
            previousValue = firstValues.Average();
            return null;
        }
        else
        {
            var smooth = smoothing / (1F + period);
            previousValue = close * smooth + previousValue * (1F - smooth);
            return previousValue;
        }
    }

    public void Reset()
    {
        length = 0;
        previousValue = 0;
        firstValues.Clear();
    }
}