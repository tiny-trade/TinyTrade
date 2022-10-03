namespace TinyTrade.Indicators;

public class Ma
{
    private List<float> firstValues;
    private int period;
    private int length;
    private float val;

    public Ma(int period = 20)
    {
        this.period = period;
        firstValues = new List<float>();
        length = 0;
        val = 0;
    }

    public float? ComputeNext(float close)
    {
        if (length < period)
        {
            firstValues.Add(close);
            length++;
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
        length = 0;
        val = 0;
        firstValues = new List<float>();
    }
}