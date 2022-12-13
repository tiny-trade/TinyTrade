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
    async Task<double> GetAvailableBalanceAsync() => await Task.Run(() => GetAvailableBalance());

    async Task WithdrawFromTradingBalanceAsync(double amount) => await Task.Run(() => WithdrawFromTradingBalance(amount));

    /// <summary>
    ///   Async version of <see cref="GetTotalBalance"/>
    /// </summary>
    /// <returns> </returns>
    async Task<double> GetTotalBalanceAsync() => await Task.Run(() => GetTotalBalance());

    /// <summary>
    ///   Async version of <see cref="OpenPosition"/>
    /// </summary>
    /// <returns> </returns>
    async Task OpenPositionAsync(OrderSide side, float openPrice, float stopLoss, float takeProfit, float bid, int leverage) => await Task.Run(() => OpenPosition(side, openPrice, stopLoss, takeProfit, bid, leverage));

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The available balance for trade </returns>
    double GetAvailableBalance();

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The total balance of the collateral </returns>
    double GetTotalBalance();

    void WithdrawFromTradingBalance(double amount);

    /// <summary>
    ///   Open a position, can be blocking
    /// </summary>
    /// <returns> </returns>
    void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float bid, int leverage);

    void Tick(DataFrame frame);

    /// <summary>
    ///   Can be blocking
    /// </summary>
    /// <returns> The number of currently opened positions </returns>
    int GetOpenPositionsNumber();
}