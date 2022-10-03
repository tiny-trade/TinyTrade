namespace TinyTrade.Indicators;

public class Stoch
{
    private readonly int fastkPeriod;
    private readonly Queue<float> lowQ;
    private readonly Queue<float> highQ;
    private readonly Ma ma;
    private readonly Ma slowDMa;

    public (float?, float?) Last { get; private set; } = (null, null);

    public Stoch(int fastkPeriod = 14, int slowkPeriod = 3, int slowdPeriod = 3)
    {
        this.fastkPeriod = fastkPeriod;
        lowQ = new Queue<float>();
        highQ = new Queue<float>();
        ma = new Ma(slowkPeriod);
        slowDMa = new Ma(slowdPeriod);
    }

    public void Reset()
    {
        Last = (null, null);
        lowQ.Clear();
        highQ.Clear();
        ma.Reset();
        slowDMa.Reset();
    }

    public (float?, float?) ComputeNext(float close, float low, float high)
    {
        if (highQ.Count < fastkPeriod)
        {
            highQ.Enqueue(high);
            lowQ.Enqueue(low);
            return (null, null);
        }
        else
        {
            highQ.Enqueue(high);
            lowQ.Enqueue(low);
            highQ.Dequeue();
            lowQ.Dequeue();

            var minLow = lowQ.Min();
            var fastk = 100F * (close - minLow) / (highQ.Max() - minLow);
            var slowK = ma.ComputeNext(fastk);
            (float?, float?) res;
            if (slowK is null)
            {
                res = (null, null);
            }
            else
            {
                var slowD = slowDMa.ComputeNext((float)slowK);
                res = slowD is null ? (null, null) : ((float?, float?))(slowK, slowD);
            }
            Last = res;
            return res;
        }
    }
}