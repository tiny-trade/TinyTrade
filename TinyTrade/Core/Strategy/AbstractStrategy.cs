using Microsoft.Extensions.Logging;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.Strategy;

internal abstract class AbstractStrategy : IStrategy
{
    private readonly ILogger logger;

    private readonly IExchange exchange;

    public int MaxConcurrentPositions { get; init; }

    protected AbstractStrategy(StrategyConstructorParameters parameters)
    {
        logger = parameters.Logger;
        exchange = parameters.Exchange;
        MaxConcurrentPositions = !parameters.Parameters.TryGetValue("maxConcurrentPositions", out var val) ? 1 : Convert.ToInt32(val);
    }

    /// <summary>
    ///   Called when a session starts
    /// </summary>
    public virtual void OnStart()
    { }

    /// <summary>
    ///   Called when a session ends
    /// </summary>
    public virtual void OnStop()
    { }

    public void UpdateState(DataFrame frame)
    {
        exchange.Tick(frame);
        if (frame.IsClosed)
        {
            Tick(frame);
            // TODO check conditions and positions
        }
    }

    protected abstract float GetStopLoss(IExchange.Side side, DataFrame frame);

    protected abstract float GetTakeProfit(IExchange.Side side, DataFrame frame);

    /// <summary>
    ///   Called each time a closed candle is received
    /// </summary>
    /// <param name="frame"> </param>
    protected virtual void Tick(DataFrame frame)
    { }
}