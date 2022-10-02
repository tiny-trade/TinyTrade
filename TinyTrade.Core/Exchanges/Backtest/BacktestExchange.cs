using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using static TinyTrade.Core.Exchanges.IExchange;

namespace TinyTrade.Core.Exchanges.Backtest;

public class BacktestExchange : IExchange
{
    private readonly ILogger logger;
    private readonly Dictionary<Guid, BacktestPosition> openPositions;
    private float balance;
    private float availableBalance;

    public List<BacktestPosition> ClosedPositions { get; private set; }

    public BacktestExchange(ILogger logger, float balance)
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