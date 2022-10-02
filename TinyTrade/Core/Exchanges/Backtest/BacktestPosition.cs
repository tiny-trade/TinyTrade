using static TinyTrade.Core.Exchanges.IExchange;

namespace TinyTrade.Core.Exchanges.Backtest;

internal struct BacktestPosition
{
    public float TakeProfit { get; init; }

    public float StopLoss { get; init; }

    public float OpenPrice { get; init; }

    public float Stake { get; init; }

    public Side Side { get; init; }

    public bool IsClosed { get; private set; }

    public bool IsWon { get; private set; }

    public float ResultPercentage { get; private set; }

    public float Profit { get; private set; }

    public BacktestPosition(Side side, float openPrice, float takeProfit, float stopLoss, float stake)
    {
        TakeProfit = takeProfit;
        StopLoss = stopLoss;
        OpenPrice = openPrice;
        Side = side;
        Stake = stake;
        IsClosed = false;
        IsWon = false;
        ResultPercentage = 0;
        Profit = 0;
    }

    public bool TryClose(float closePrice)
    {
        switch (Side)
        {
            case Side.Buy:
                if (closePrice < StopLoss || closePrice > TakeProfit)
                {
                    IsClosed = true;
                    IsWon = closePrice >= TakeProfit;
                    ResultPercentage = (closePrice / OpenPrice) - 1;
                    Profit = Stake * ResultPercentage;
                    return true;
                }
                break;

            case Side.Sell:
                if (closePrice < TakeProfit || closePrice > StopLoss)
                {
                    IsClosed = true;
                    IsWon = closePrice <= TakeProfit;
                    ResultPercentage = (OpenPrice / closePrice) - 1;
                    Profit = Stake * ResultPercentage;
                    return true;
                }
                break;

            default:
                return false;
        }
        return false;
    }
}