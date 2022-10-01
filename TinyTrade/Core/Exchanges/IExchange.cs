namespace TinyTrade.Core.Exchanges;

internal interface IExchange
{
    public enum Side { Buy, Sell }

    Task<int> GetOpenPositionsNumber();

    Task<float> GetAvailableBalance();

    Task<float> GetTotalBalance();

    Task OpenPosition(string pair, Side side, float openPrice, float stopLoss, float takeProfit, float bid);

    void Tick(DataFrame frame);
}