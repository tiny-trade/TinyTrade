using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Exchanges.Offline;

/// <summary>
///   An exchange that keeps a dummy balance and utomatically check for <see cref="OfflinePosition"/> status in the <see
///   cref="Tick(DataFrame)"/> method. It overrides the <see cref="IExchange"/> async methods in order to provide a faster processing:
///   methods are treated as synchronous since there is no need for any endpoint call
/// </summary>
public class OfflineExchange : IExchange
{
    private readonly ILogger? logger;
    private readonly Dictionary<Guid, OfflinePosition> openPositions;
    private readonly double operationFee;
    private double balance;
    private double availableBalance;

    public double TotalFees { get; private set; } = 0D;

    public float InitialBalance { get; private set; }

    public List<OfflinePosition> ClosedPositions { get; private set; }

    public OfflineExchange(float balance = 100, double operationFee = 0.001D, ILogger? logger = null)
    {
        openPositions = new Dictionary<Guid, OfflinePosition>();
        this.logger = logger;
        this.balance = balance;
        this.operationFee = operationFee;
        availableBalance = balance;
        InitialBalance = balance;
        ClosedPositions = new List<OfflinePosition>();
    }

    public void Reset()
    {
        balance = InitialBalance;
        availableBalance = InitialBalance;
        openPositions.Clear();
    }

    public void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float margin, int leverage)
    {
        if (availableBalance < margin || availableBalance < 0) return;
        availableBalance -= margin;
        PayFee(margin);
        var pos = new OfflinePosition(side, openPrice, takeProfit, stopLoss, margin, leverage);
        openPositions.Add(Guid.NewGuid(), pos);
    }

    async Task<double> IExchange.GetTotalBalanceAsync()
    {
        await Task.CompletedTask;
        return GetTotalBalance();
    }

    async Task<double> IExchange.GetAvailableBalanceAsync()
    {
        await Task.CompletedTask;
        return GetAvailableBalance();
    }

    async Task<int> IExchange.GetOpenPositionsNumberAsync()
    {
        await Task.CompletedTask;
        return GetOpenPositionsNumber();
    }

    async Task IExchange.OpenPositionAsync(OrderSide side, float openPrice, float stopLoss, float takeProfit, float stake, int leverage)
    {
        await Task.CompletedTask;
        OpenPosition(side, openPrice, stopLoss, takeProfit, stake, leverage);
    }

    public void Tick(DataFrame dataFrame)
    {
        var remove = new List<Guid>();
        for (var i = 0; i < openPositions.Count; i++)
        {
            var p = openPositions.ElementAt(i);
            if (p.Value.TryClose(dataFrame.Close))
            {
                balance += p.Value.NetProfit;
                PayFee(p.Value.Margin);
                availableBalance += p.Value.Margin + p.Value.NetProfit;
                remove.Add(p.Key);
                ClosedPositions.Add(p.Value);
            }
        }
        foreach (var i in remove)
        {
            openPositions.Remove(i);
        }
    }

    public double GetAvailableBalance() => availableBalance;

    public int GetOpenPositionsNumber() => openPositions.Count;

    public double GetTotalBalance() => balance;

    private void PayFee(float margin)
    {
        var fee = margin * operationFee;
        TotalFees += fee;
        balance -= fee;
    }
}