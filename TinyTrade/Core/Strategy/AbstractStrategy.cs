using Microsoft.Extensions.Logging;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.Strategy;

internal abstract class AbstractStrategy : IStrategy
{
    private readonly ILogger logger;

    private readonly IExchange exchange;
    private readonly List<Condition> shortConditions;
    private readonly List<Condition> longConditions;

    public int MaxConcurrentPositions { get; init; }

    protected AbstractStrategy(StrategyConstructorParameters parameters)
    {
        logger = parameters.Logger;
        exchange = parameters.Exchange;
        MaxConcurrentPositions = !parameters.Parameters.TryGetValue("maxConcurrentPositions", out var val) ? 1 : Convert.ToInt32(val);
        shortConditions = new List<Condition>();
        longConditions = new List<Condition>();
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
            foreach (var c in shortConditions)
            {
                c.Tick(frame);
            }
            foreach (var c in longConditions)
            {
                c.Tick(frame);
            }

            var openPositions = exchange.GetOpenPositionsNumber();
            var balance = exchange.GetAvailableBalance();
            if (openPositions < MaxConcurrentPositions && balance > 0)
            {
                var stake = GetStakeAmount();

                if (longConditions.Count > 0 && longConditions.All(c => c.IsSatisfied))
                {
                    var side = IExchange.Side.Buy;

                    exchange.OpenPosition(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), stake);
                    longConditions.ForEach(c => c.Reset());
                }

                if (shortConditions.Count > 0 && shortConditions.All(c => c.IsSatisfied))
                {
                    var side = IExchange.Side.Sell;

                    exchange.OpenPosition(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), stake);
                    shortConditions.ForEach(c => c.Reset());
                }
            }
        }
    }

    protected abstract float GetStakeAmount();

    protected void AddShortCondition(Condition condition)
    {
        if (shortConditions.Contains(condition)) return;
        shortConditions.Add(condition);
    }

    protected void AddLongCondition(Condition condition)
    {
        if (longConditions.Contains(condition)) return;
        longConditions.Add(condition);
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