using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Live.Modes;

internal class ForetestRun : BaseRun
{
    public ForetestRun(Pair pair, Timeframe timeframe, IStrategy strategy, IExchange exchange, ILogger? logger = null) : base(pair, timeframe, strategy, exchange, logger)
    {
    }

    protected override async void Heartbeat(DataFrame frame) => Logger?.Log(LogLevel.Information, "\nKline: {k}\nTotal balance: {b}\n", frame.ToString(), await ExchangeInterface.GetTotalBalanceAsync());
}