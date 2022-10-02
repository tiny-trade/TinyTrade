namespace TinyTrade.Indicators;

internal class Stoch
{
    private int fastkPeriod;
    private int slowkPeriod;
    private int slowdPeriod;
    private Queue<float?> stoch;
    private Queue<float?> closeQ;
    private Queue<float?> highQ;
    private Queue<float?> lowQ;
    private Queue<float?> fastk;
    private Queue<float?> slowd;

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
        diff = (maxVal - minVal);
        if (diff == 0) diff = 1;

        lastK = ((close - minVal) / diff) * 100;
        stoch.Enqueue(lastK);

        if (stoch.Count == fastkPeriod)
        {
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
            if (slowd.Count > slowkPeriod)
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