using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges.Offline;

namespace TinyTrade.Core.Models;

[Serializable]
public struct BacktestResultModel
{
    public float Days { get; private set; }

    public string Timeframe { get; private set; }

    public int Candles { get; private set; }

    public int Frames { get; private set; }

    public long ElapsedMillis { get; private set; }

    public double InitialBalance { get; private set; }

    public double FinalBalance { get; private set; }

    public double Profit { get; private set; }

    public float WinRate { get; private set; }

    public double EstimatedApy { get; private set; }

    public int ClosedPositions { get; private set; }

    public double ProfitPercentage { get; private set; }

    public double TotalFees { get; private set; }

    public int LiquidatedPositions { get; private set; }

    public BacktestResultModel(List<OfflinePosition> positions, Timeframe timeframe, double initialBalance, double finalBalance, double totalFees, int candles, long elapsedMillis)
    {
        Frames = candles * timeframe.Minutes;
        Candles = candles;
        ElapsedMillis = elapsedMillis;
        Timeframe = timeframe;
        TotalFees = totalFees;
        Days = candles / 1440;
        InitialBalance = initialBalance;
        FinalBalance = finalBalance;
        Profit = FinalBalance - InitialBalance;
        ProfitPercentage = 100F * ((FinalBalance / InitialBalance) - 1F);
        float won = positions.Count(p => p.IsWon);
        WinRate = positions.Count <= 0 ? 0 : won / positions.Count;
        float totPerc = 0;
        positions.ForEach(p => totPerc += p.ResultRatio);
        totPerc = positions.Count <= 0 ? 0 : totPerc / positions.Count;
        ClosedPositions = positions.Count;
        EstimatedApy = (MathF.Pow((float)(FinalBalance / InitialBalance), 365F / Days) - 1F) * 100;
        EstimatedApy = EstimatedApy < -100 ? -100 : EstimatedApy;
        LiquidatedPositions = positions.Count(p => p.Liquidated);
    }
}