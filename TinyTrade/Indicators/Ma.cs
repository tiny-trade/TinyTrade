namespace TinyTrade.Indicators;

internal class Ma
{
    private List<float> firstValues;
    private int period;
    private int lenght;
    private float val;

    public Ma(int period = 20)
    {
        this.period = period;
        firstValues = new List<float>();
        lenght = 0;
        val = 0;
    }

    public float? ComputeNext(float close)
    {
        if (lenght < period)
        {
            firstValues.Add(close);
            lenght++;
            return null;
        }
        else
        {
            firstValues.RemoveAt(firstValues.Count - 1);
            firstValues.Add(close);
            val = firstValues.Sum();
            return val / period;
        }
    }

    public void Reset()
    {
        lenght = 0;
        val = 0;
        firstValues = new List<float>();
    }
}