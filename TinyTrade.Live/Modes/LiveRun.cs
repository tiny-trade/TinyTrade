using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Strategy;

namespace TinyTrade.Live.Modes;

internal class LiveRun : BaseRun
{
    public LiveRun(Pair pair, Timeframe timeframe, IStrategy strategy, IExchange exchange, ILogger? logger = null) : base(pair, timeframe, strategy, exchange, logger)
    {
    }
}