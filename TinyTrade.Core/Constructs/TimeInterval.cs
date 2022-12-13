namespace TinyTrade.Core.Constructs;

/// <summary>
/// Class encapsulating the creation and management of a time interval
/// </summary>
public class TimeInterval
{
    private int fromYear;
    private int toYear;
    private int fromMonth;
    private int toMonth;

    public int FromYear => fromYear;

    public int ToYear => toYear;

    public int FromMonth => fromMonth;

    public int ToMonth => toMonth;

    public int MonthsInterval => (toYear - fromYear) * 12 + (toMonth - fromMonth);

    public TimeInterval(string pattern)
    {
        Parse(pattern);
    }

    public static implicit operator TimeInterval(string pattern) => new TimeInterval(pattern);

    public IEnumerable<string> GetPeriods()
    {
        var periods = new List<string>();
        var year = fromYear;
        var month = fromMonth;
        while (year <= toYear)
        {
            var targetMonth = year == toYear ? toMonth : 12;
            while (month <= targetMonth)
            {
                periods.Add(year.ToString("0000") + "-" + month.ToString("00"));
                month++;
            }
            month = 1;
            year++;
        }
        return periods;
    }

    public override string? ToString() => fromYear.ToString("0000") + "-" + fromMonth.ToString("00") + "_" + toYear.ToString("0000") + "-" + toMonth.ToString("00");

    private void Parse(string pattern)
    {
        EmptyInterval();
        var pieces = pattern.Split("|");
        var from = pieces[0].Split("-");
        if (from.Length < 2 || !int.TryParse(from[0], out fromYear) || !int.TryParse(from[1], out fromMonth))
        {
            EmptyInterval();
            return;
        }
        if (pieces.Length < 2) return;

        var to = pieces[1].Split("-");
        if (to.Length < 2 || !int.TryParse(to[0], out toYear) || !int.TryParse(to[1], out toMonth))
        {
            EmptyInterval();
            return;
        }
        if (fromYear > toYear)
        {
            EmptyInterval();
            return;
        }
    }

    private void EmptyInterval()
    {
        var now = DateTime.Now;
        fromYear = now.Year;
        toYear = now.Year;
        fromMonth = now.Month;
        toMonth = now.Month - 1;
        if (toMonth < 1)
        {
            toYear--;
            toMonth = 12;
        }
    }
}