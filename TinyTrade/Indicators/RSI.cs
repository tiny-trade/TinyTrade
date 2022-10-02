using System.Security;

namespace TinyTrade.Indicators;

internal class Rsi
{
    private int period;
    private int counter;
    private float prev;
    private float gain;
    private float loss;
    private Queue<float> firstCloses;

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
            if (firstCloses.Count == period + 2)
            {
                firstCloses.Dequeue();
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
        }

        prev = close;
        if (counter < period + 1)
        {
            return null;
        }
        return 100 - (100 / (1 + (gain / loss)));
    }

    private (float, float) AvgGainLoss(Queue<float> closes)
    {
        List<float> gain = new List<float>();
        List<float> loss = new List<float>();
        float prev = closes.Peek();
        if (closes.Count == 1)
        {
            return (0, 0);
        }
        for (int i = 1; i < closes.Count; i++)
        {
            float current = closes.ElementAt(i);
            if (prev <= current)
            {
                gain.Add(current - prev);
                loss.Add(0);
            }
            else if (prev > current)
            {
                gain.Add(0);
                loss.Add(prev - current);
            }
            prev = current;
        }
        return (gain.Average(), loss.Average());
    }
}