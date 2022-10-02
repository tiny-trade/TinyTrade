namespace TinyTrade.Core.Exchanges;

internal interface IExchange
{
    public enum Side { Buy, Sell }

    int GetOpenPositionsNumber();

    float GetAvailableBalance();

    float GetTotalBalance();

    void OpenPosition(Side side, float openPrice, float stopLoss, float takeProfit, float bid);

    void Tick(DataFrame frame);
}