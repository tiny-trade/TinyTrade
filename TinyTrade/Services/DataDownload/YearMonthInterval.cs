namespace TinyTrade.Services.DataDownload;

internal class YearMonthInterval
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

    public YearMonthInterval(string pattern)
    {
        Parse(pattern);
    }

    public IEnumerable<string> Periods()
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
            month = 0;
            year++;
        }
        return periods;
    }

    private void Parse(string pattern)
    {
        Now();
        var pieces = pattern.Split("|");
        var from = pieces[0].Split("-");
        if (from.Length < 2 || !int.TryParse(from[0], out fromYear) || !int.TryParse(from[1], out fromMonth))
        {
            Now();
            return;
        }
        if (pieces.Length < 2) return;

        var to = pieces[1].Split("-");
        if (to.Length < 2 || !int.TryParse(to[0], out toYear) || !int.TryParse(to[1], out toMonth))
        {
            Now();
            return;
        }
    }

    private void Now()
    {
        var now = DateTime.Now;
        fromYear = now.Year;
        toYear = now.Year;
        fromMonth = now.Month;
        toMonth = now.Month - 1;
        if (toMonth < 0)
        {
            toYear--;
            toMonth = 12;
        }
    }
}