namespace TinyTrade.Indicators;

public class Rsi
{
    private readonly int period;
    private readonly Queue<float> firstCloses;
    private int counter;
    private float prev;
    private float gain;
    private float loss;

    public float? Last { get; private set; } = null;

    public Rsi(int period = 14)
    {
        this.period = period;
        firstCloses = new Queue<float>();
        counter = 0;
        prev = 0;
        gain = 0;
        loss = 0;
    }

    public float? ComputeNext(float close)
    {
        if (counter <= period)
        {
            counter++;
            firstCloses.Enqueue(close);
            if (firstCloses.Count > period + 1)
            {
                firstCloses.Dequeue();
            }
            (gain, loss) = AvgGainLoss(firstCloses);
        }
        else
        {
            if (prev <= close)
            {
                gain = (gain * (period - 1) + (close - prev)) / period;
                loss = (loss * (period - 1)) / period;
            }
            else if (prev > close)
            {
                loss = (loss * (period - 1) + (prev - close)) / period;
                gain = (gain * (period - 1)) / period;
            }
        }

        prev = close;
        float? res = counter < period + 1 ? null : 100F - (100F / (1 + (gain / loss)));
        Last = res;
        return res;
    }

    public void Reset()
    {
        Last = null;
        firstCloses.Clear();
        counter = 0;
        prev = 0;
        gain = 0;
        loss = 0;
    }

    private (float, float) AvgGainLoss(Queue<float> closes)
    {
        var gain = new List<float>();
        var loss = new List<float>();
        var prev = closes.Peek();
        if (closes.Count == 1)
        {
            return (0, 0);
        }
        for (var i = 1; i < closes.Count; i++)
        {
            var current = closes.ElementAt(i);
            if (prev <= current)
            {
                gain.Add(current - prev);
                loss.Add(0F);
            }
            else if (prev > current)
            {
                gain.Add(0F);
                loss.Add(prev - current);
            }
            prev = current;
        }
        return (gain.Average(), loss.Average());
    }
}