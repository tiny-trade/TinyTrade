using Newtonsoft.Json;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges.Backtest;

namespace TinyTrade.Core.Models;

[Serializable]
public struct BacktestResultModel
{
    [JsonProperty("days")]
    public float Days { get; private set; }

    [JsonProperty("timeframe")]
    public string Timeframe { get; private set; }

    [JsonProperty("initialBalance")]
    public float InitialBalance { get; private set; }

    [JsonProperty("finalBalance")]
    public float FinalBalance { get; private set; }

    [JsonProperty("profit")]
    public float Profit { get; private set; }

    [JsonProperty("winRate")]
    public float WinRate { get; private set; }

    [JsonProperty("estimatedApy")]
    public float EstimatedApy { get; private set; }

    [JsonProperty("closedPositions")]
    public int ClosedPositions { get; private set; }

    [JsonProperty("profitPercentage")]
    public float ProfitPercentage { get; private set; }

    public BacktestResultModel(List<BacktestPosition> positions, Timeframe timeframe, float initialBalance, float finalBalance, int candles)
    {
        Timeframe = timeframe;
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
        EstimatedApy = (MathF.Pow(FinalBalance / InitialBalance, 365F / Days) - 1F) * 100;
        EstimatedApy = EstimatedApy < -100 ? -100 : EstimatedApy;
    }
}