using Microsoft.Extensions.Logging;
using TinyTrade.Core.Exchanges.Offline;
using TinyTrade.Core.Shared;

namespace TinyTrade.Core.Exchanges;


public static class ExchangeFactory
{
    /// <summary>
    /// </summary>
    /// <param name="initialBalance"> </param>
    /// <param name="logger"> </param>
    /// <returns> An exchange that is used for testing purposes </returns>
    public static OfflineExchange GetLocalTestExchange(float initialBalance, ILogger? logger = null)
        => new OfflineExchange(initialBalance, logger);

    public static IExchange GetExchange(Exchange exchange, ILogger? logger = null)
    {
        switch (exchange)
        {
            case Exchange.Kucoin:
                return null!;

            default:
                return null!;
        }
    }
}