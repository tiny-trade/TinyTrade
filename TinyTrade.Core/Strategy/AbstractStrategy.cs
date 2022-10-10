using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.Strategy;

public abstract class AbstractStrategy : IStrategy
{
    private readonly List<Condition> shortConditions;

    private readonly List<Condition> longConditions;

    public int MaxConcurrentPositions { get; init; }

    protected ILogger? Logger { get; private set; }

    protected IExchange Exchange { get; private set; }

    protected AbstractStrategy(StrategyConstructorParameters parameters)
    {
        Logger = parameters.Logger;
        Exchange = parameters.Exchange;
        MaxConcurrentPositions = Convert.ToInt32(parameters.Parameters.GetValueOrDefault("maxConcurrentPositions", 1));
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

    /// <summary>
    ///   Reset the status of all conditions of the strategy
    /// </summary>
    public virtual void Reset()
    {
        foreach (var c in longConditions)
        {
            c.Reset();
        }
        foreach (var c in shortConditions)
        {
            c.Reset();
        }
    }

    public async Task UpdateState(DataFrame frame)
    {
        Exchange.Tick(frame);
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

            var posTask = Exchange.GetOpenPositionsNumberAsync();
            var balanceTask = Exchange.GetAvailableBalanceAsync();
            await Task.WhenAll(posTask, balanceTask);
            var openPositions = posTask.Result;
            var balance = balanceTask.Result;
            if (openPositions < MaxConcurrentPositions && balance > 0)
            {
                var stake = balance * GetStakeAmount();

                if (longConditions.Count > 0 && longConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Buy;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), stake);
                    longConditions.ForEach(c => c.Reset());
                }

                if (shortConditions.Count > 0 && shortConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Sell;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), stake);
                    shortConditions.ForEach(c => c.Reset());
                }
            }
        }
    }

    protected abstract float GetStakeAmount();

    protected AbstractStrategy AddShortCondition(Condition condition)
    {
        if (!shortConditions.Contains(condition))
        {
            shortConditions.Add(condition);
        }
        return this;
    }

    protected AbstractStrategy AddLongCondition(Condition condition)
    {
        if (!longConditions.Contains(condition))
        {
            longConditions.Add(condition);
        }
        return this;
    }

    protected abstract float GetStopLoss(OrderSide side, DataFrame frame);

    protected abstract float GetTakeProfit(OrderSide side, DataFrame frame);

    /// <summary>
    ///   Called each time a closed candle is received
    /// </summary>
    /// <param name="frame"> </param>
    protected virtual void Tick(DataFrame frame)
    { }
}