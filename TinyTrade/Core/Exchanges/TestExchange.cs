using Microsoft.Extensions.Logging;
using static TinyTrade.Core.Exchanges.IExchange;

namespace TinyTrade.Core.Exchanges;

internal class TestExchange : IExchange
{
    private readonly ILogger logger;
    private readonly Dictionary<Guid, TestPosition> openPositions;
    private float balance;
    private float availableBalance;

    public TestExchange(ILogger logger, int balance)
    {
        openPositions = new Dictionary<Guid, TestPosition>();
        this.logger = logger;
        this.balance = balance;
        availableBalance = balance;
    }

    public async Task<float> GetAvailableBalance()
    {
        await Task.CompletedTask;
        return availableBalance;
    }

    public async Task<int> GetOpenPositionsNumber()
    {
        await Task.CompletedTask;
        return openPositions.Count;
    }

    public async Task<float> GetTotalBalance()
    {
        await Task.CompletedTask;
        return balance;
    }

    public async Task OpenPosition(string pair, Side side, float openPrice, float stopLoss, float takeProfit, float stake)
    {
        if (availableBalance < stake) return;
        availableBalance -= stake;
        var pos = new TestPosition(side, openPrice, takeProfit, stopLoss, stake);
        openPositions.Add(Guid.NewGuid(), pos);
        await Task.CompletedTask;
    }

    public void Tick(DataFrame dataFrame)
    {
        var remove = new List<Guid>();
        for (var i = 0; i < openPositions.Count; i++)
        {
            var p = openPositions.ElementAt(i);
            if (p.Value.TryClose(dataFrame.Close, out var profit))
            {
                balance += profit;
                availableBalance += p.Value.Stake + profit;
                remove.Add(p.Key);
            }
        }
        foreach (var i in remove)
        {
            openPositions.Remove(i);
        }
    }

    private struct TestPosition
    {
        public float TakeProfit { get; init; }

        public float StopLoss { get; init; }

        public float OpenPrice { get; init; }

        public float Stake { get; init; }

        public Side Side { get; init; }

        public TestPosition(Side side, float openPrice, float takeProfit, float stopLoss, float stake)
        {
            TakeProfit = takeProfit;
            StopLoss = stopLoss;
            OpenPrice = openPrice;
            Side = side;
            Stake = stake;
        }

        public bool TryClose(float closePrice, out float profit)
        {
            profit = 0F;
            switch (Side)
            {
                case Side.Buy:
                    if (closePrice < StopLoss || closePrice > TakeProfit)
                    {
                        profit = ((closePrice / OpenPrice) - 1) * 100;
                        profit = Stake * (profit / 100F);
                        return true;
                    }
                    break;

                case Side.Sell:
                    if (closePrice < TakeProfit || closePrice > StopLoss)
                    {
                        profit = ((OpenPrice / closePrice) - 1) * 100;
                        profit = Stake * (profit / 100F);
                        return true;
                    }
                    break;

                default:
                    return false;
            }
            return false;
        }
    }
}