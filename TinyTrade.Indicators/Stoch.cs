namespace TinyTrade.Indicators;

public class Stoch
{
    private readonly int fastkPeriod;
    private readonly int slowkPeriod;
    private readonly int slowdPeriod;
    private readonly Queue<float?> stoch;
    private readonly Queue<float?> closeQ;
    private readonly Queue<float?> highQ;
    private readonly Queue<float?> lowQ;
    private readonly Queue<float?> fastk;
    private readonly Queue<float?> slowd;

    public Stoch(int fastkPeriod = 14, int slowkPeriod = 3, int slowdPeriod = 3)
    {
        this.fastkPeriod = fastkPeriod;
        this.slowkPeriod = slowkPeriod;
        this.slowdPeriod = slowdPeriod;

        stoch = new Queue<float?>();
        closeQ = new Queue<float?>();
        highQ = new Queue<float?>();
        lowQ = new Queue<float?>();
        fastk = new Queue<float?>();
        slowd = new Queue<float?>();
    }

    public (float?, float?) ComputeNext(float? close, float? low, float? high)
    {
        float? minVal;
        float? maxVal;
        float? diff;
        float? lastK;
        float? newK;
        float? stochD;

        highQ.Enqueue(high);
        if (highQ.Count > fastkPeriod)
        {
            highQ.Dequeue();
        }
        lowQ.Enqueue(low);
        if (lowQ.Count > fastkPeriod)
        {
            lowQ.Dequeue();
        }
        closeQ.Enqueue(close);
        if (closeQ.Count > fastkPeriod)
        {
            closeQ.Dequeue();
        }
        minVal = lowQ.Min();
        maxVal = highQ.Max();
        diff = maxVal - minVal;
        if (diff == 0) diff = 1F;

        lastK = ((close - minVal) / diff) * 100;
        stoch.Enqueue(lastK);

        if (stoch.Count >= fastkPeriod)
        {
            stoch.Dequeue();
            fastk.Enqueue(lastK);
            if (fastk.Count > slowkPeriod)
            {
                fastk.Dequeue();
                newK = fastk.Sum() / slowkPeriod;
            }
            else
            {
                newK = null;
            }

            slowd.Enqueue(newK);
            if (slowd.Count > slowdPeriod)
            {
                slowd.Dequeue();
                stochD = slowd.Average();
            }
            else
            {
                stochD = null;
            }
            return (newK, stochD);
        }
        else
        {
            return (null, null);
        }
    }
}