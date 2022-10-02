using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Exchanges;

public enum OrderSide
{ Buy, Sell }

public interface IExchange
{
    int GetOpenPositionsNumber();

    float GetAvailableBalance();

    float GetTotalBalance();

    void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float bid);

    void Tick(DataFrame frame);
}