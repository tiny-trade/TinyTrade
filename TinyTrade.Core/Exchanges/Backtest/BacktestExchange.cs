using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Exchanges.Backtest;

/// <summary>
///   Test exchange based on backtest data. It overrides the <see cref="IExchange"/> async methods in order to provide a faster processing:
///   methods are treated as synchronous since there is no need for any endpoint call
/// </summary>
public class LocalTestExchange : IExchange
{
    private readonly ILogger? logger;
    private readonly Dictionary<Guid, BacktestPosition> openPositions;
    private float balance;
    private float availableBalance;

    public List<BacktestPosition> ClosedPositions { get; private set; }

    internal LocalTestExchange(float balance, ILogger? logger = null)
    {
        openPositions = new Dictionary<Guid, BacktestPosition>();
        this.logger = logger;
        this.balance = balance;
        availableBalance = balance;
        ClosedPositions = new List<BacktestPosition>();
    }

    public void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float stake)
    {
        if (availableBalance < stake) return;
        availableBalance -= stake;
        var pos = new BacktestPosition(side, openPrice, takeProfit, stopLoss, stake);
        openPositions.Add(Guid.NewGuid(), pos);
    }

    async Task<float> IExchange.GetTotalBalanceAsync()
    {
        await Task.CompletedTask;
        return GetTotalBalance();
    }

    async Task<float> IExchange.GetAvailableBalanceAsync()
    {
        await Task.CompletedTask;
        return GetAvailableBalance();
    }

    async Task<int> IExchange.GetOpenPositionsNumberAsync()
    {
        await Task.CompletedTask;
        return GetOpenPositionsNumber();
    }

    async Task IExchange.OpenPositionAsync(OrderSide side, float openPrice, float stopLoss, float takeProfit, float stake)
    {
        await Task.CompletedTask;
        OpenPosition(side, openPrice, stopLoss, takeProfit, stake);
    }

    public void Tick(DataFrame dataFrame)
    {
        var remove = new List<Guid>();
        for (var i = 0; i < openPositions.Count; i++)
        {
            var p = openPositions.ElementAt(i);
            if (p.Value.TryClose(dataFrame.Close))
            {
                balance += p.Value.Profit;
                availableBalance += p.Value.Stake + p.Value.Profit;
                remove.Add(p.Key);
                ClosedPositions.Add(p.Value);
            }
        }
        foreach (var i in remove)
        {
            openPositions.Remove(i);
        }
    }

    public float GetAvailableBalance() => availableBalance;

    public int GetOpenPositionsNumber() => openPositions.Count;

    public float GetTotalBalance() => balance;
}