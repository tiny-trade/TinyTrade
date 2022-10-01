namespace TinyTrade.Indicators;

internal class BollingerBands
{
    private int stdev;
    private int period;
    private int length;
    private List<float> firstValues;
    private Ma ma;

    public BollingerBands(int period = 20, int stdev = 2)
    {
        this.stdev = stdev;
        ma = new Ma(period);
        length = 0;
        firstValues = new List<float>();
    }

    public (float?, float?, float?) ComputeNext(float close)
    {
        float? currentMa = ma.ComputeNext(close);
        float currentStdev;
        if (length < period)
        {
            length++;
            firstValues.Add(close);
            return (null, null, null);
        }
        else
        {
            if (currentMa == null) return (null, null, null);
            currentStdev = CalculateStdev((float)currentMa);
            return (currentMa, currentMa + (currentStdev * stdev), currentMa - (currentStdev * stdev));
        }
    }

    public void Reset()
    {
        length = 0;
        firstValues = new List<float>();
        ma.Reset();
    }

    private float CalculateStdev(float mean)
    {
        double cumulative = 0;
        foreach (float v in firstValues)
        {
            cumulative += Math.Pow(v - mean, 2);
        }
        cumulative /= length;
        return (float)Math.Sqrt(cumulative);
    }
}