namespace TinyTrade.Core.Exchanges.Offline;

/// <summary>
/// Model identifying a position taken in a offline environment (position on exchanges are not kept locally)
/// </summary>
[Serializable]
public class OfflinePosition
{
    public float TakeProfit { get; init; }

    public float StopLoss { get; init; }

    public float OpenPrice { get; init; }

    public int Leverage { get; init; }

    public float Margin { get; init; }

    public OrderSide Side { get; init; }

    public bool IsClosed { get; private set; }

    public double CounterValue { get; private set; }

    public bool IsWon { get; private set; }

    public float LiquidationPrice { get; private set; }

    public bool Liquidated { get; private set; }

    public float ResultRatio { get; private set; }

    public float Fee { get; private set; }

    public double NetProfit { get; private set; }

    public OfflinePosition(OrderSide side, float openPrice, float takeProfit, float stopLoss, float margin, int leverage = 1)
    {
        TakeProfit = takeProfit;
        StopLoss = stopLoss;
        OpenPrice = openPrice;
        Side = side;
        Leverage = leverage;
        Margin = margin;
        CounterValue = margin * leverage;
        IsClosed = false;
        Liquidated = false;
        IsWon = false;
        LiquidationPrice = side == OrderSide.Buy ? openPrice - openPrice / leverage : openPrice + openPrice / leverage;
        ResultRatio = 0;
        NetProfit = 0;
    }

    public bool TryClose(float closePrice)
    {
        // Handle liquidation
        if ((Side is OrderSide.Buy && closePrice <= LiquidationPrice) || (Side is OrderSide.Sell && closePrice >= LiquidationPrice))
        {
            IsClosed = true;
            IsWon = false;
            ResultRatio = (LiquidationPrice / OpenPrice) - 1F;
            if (Side is OrderSide.Sell) ResultRatio *= -1;
            NetProfit = CounterValue * ResultRatio;
            Liquidated = true;
            return true;
        }

        switch (Side)
        {
            case OrderSide.Buy:
                if (closePrice <= StopLoss || closePrice >= TakeProfit)
                {
                    IsClosed = true;
                    IsWon = closePrice >= TakeProfit;
                    ResultRatio = ((IsWon ? TakeProfit : StopLoss) / OpenPrice) - 1F;

                    NetProfit = CounterValue * ResultRatio;
                    return true;
                }
                break;

            case OrderSide.Sell:
                if (closePrice <= TakeProfit || closePrice >= StopLoss)
                {
                    IsClosed = true;
                    IsWon = closePrice <= TakeProfit;
                    ResultRatio = (OpenPrice / (IsWon ? TakeProfit : StopLoss)) - 1F;
                    NetProfit = CounterValue * ResultRatio;
                    return true;
                }
                break;

            default:
                return false;
        }
        return false;
    }
}