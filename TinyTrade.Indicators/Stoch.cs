namespace TinyTrade.Indicators;

public class Stoch
{
    private readonly int fastkPeriod;
    private readonly int slowkPeriod;
    private readonly int slowdPeriod;
    private Queue<float> lowQ;
    private Queue<float> highQ;
    private Queue<float> stochQ;
    private Ma ma;
    private Ma slowDMa;

    public Stoch(int fastkPeriod = 14, int slowkPeriod = 3, int slowdPeriod = 3)
    {
        this.fastkPeriod = fastkPeriod;
        this.slowkPeriod = slowkPeriod;
        this.slowdPeriod = slowdPeriod;
        lowQ = new Queue<float>();
        highQ = new Queue<float>();
        stochQ = new Queue<float>();
        ma = new Ma(slowkPeriod);
        slowDMa = new Ma(slowdPeriod);
    }

    public (float?, float?) ComputeNext(float close, float low, float high)
    {
        (float?, float?) res = (0F, 0F);
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
            if (slowK is null)
            {
                res = (null, null);
            }
            else
            {
                var slowD = slowDMa.ComputeNext((float)slowK);
                res = slowD is null ? (null, null) : ((float?, float?))(slowK, slowD);
            }
            return res;
        }
    }
}