using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Exchanges;

public enum OrderSide
{ Buy, Sell }

public interface IExchange
{
    /// <summary>
    ///   Async version of <see cref="GetOpenPositionsNumber"/>
    /// </summary>
    /// <returns> </returns>
    async Task<int> GetOpenPositionsNumberAsync() => await Task.Run(() => GetOpenPositionsNumber());

    /// <summary>
    ///   Async version of <see cref="GetAvailableBalance"/>
    /// </summary>
    /// <returns> </returns>
    async Task<float> GetAvailableBalanceAsync() => await Task.Run(() => GetAvailableBalance());

    /// <summary>
    ///   Async version of <see cref="GetTotalBalance"/>
    /// </summary>
    /// <returns> </returns>
    async Task<float> GetTotalBalanceAsync() => await Task.Run(() => GetTotalBalance());

    /// <summary>
    ///   Async version of <see cref="OpenPosition"/>
    /// </summary>
    /// <returns> </returns>
    async Task OpenPositionAsync(OrderSide side, float openPrice, float stopLoss, float takeProfit, float bid) => await Task.Run(() => OpenPosition(side, openPrice, stopLoss, takeProfit, bid));

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The available balance for trade </returns>
    float GetAvailableBalance();

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The total balance of the collateral </returns>
    float GetTotalBalance();

    /// <summary>
    ///   Open a position, can be blocking
    /// </summary>
    /// <returns> </returns>
    void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float bid);

    void Tick(DataFrame frame);

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The number of currently opened positions </returns>
    int GetOpenPositionsNumber();
}