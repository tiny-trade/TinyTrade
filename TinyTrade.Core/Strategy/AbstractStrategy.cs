using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Indicators;

namespace TinyTrade.Core.Strategy;

/// <summary>
/// Base class defining the behaviour of a strategy. 
/// All implemented strategy are encouraged to inherit from this class and to not implement directly the interface <see cref="IStrategy"/>, 
/// unless extremely necessary, for example in order to define completely different strategy logic
/// </summary>
public abstract class AbstractStrategy : IStrategy
{
    private readonly List<AbstractCondition> shortConditions;

    private readonly List<AbstractCondition> longConditions;
    private IEnumerable<Indicator>? indicators = null;
    private double? thresholdBalance;

    /// <summary>
    /// Maximum number of concurrently opened positions
    /// </summary>
    public int MaxConcurrentPositions { get; init; }

    /// <summary>
    /// Select a threshold defined in decimal ratio after which the strategy will perform a withdraw from a the trading account calling <see cref="IExchange.WithdrawFromTradingBalance(double)"/>
    /// <para>
    /// Example: <see cref="WithdrawThreshold"/> = 0.02 > after having gained 2%, the withdrawal request of an amount defined by <see cref="WithdrawRatio"/> will be performed
    /// </para>
    /// </summary>
    public double WithdrawThreshold { get; init; } = 0D;

    /// <summary>
    /// Select a decimal ratio to withdraw after having reached a relative gain of <see cref="WithdrawThreshold"/>
    /// <para>
    /// Example, <see cref="WithdrawRatio"/> = 0.05, <see cref="WithdrawThreshold"/> = 0.02, initial balance = 1000
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description>After having reached a balance of <b>1050</b>, withdraw (secure) <b>20</b></description></item>
    /// <item><description>Next withdrawal will be now triggered only once reached <b>1102.5 = 1050 * <see cref="WithdrawThreshold"/></b> 
    /// and will be of <b>22.05 = 1050 * <see cref="WithdrawRatio"/></b></description></item>
    /// </list>
    /// </para>
    /// </summary>
    public double WithdrawRatio { get; init; } = 0D;

    protected ILogger? Logger { get; private set; }

    /// <summary>
    /// Current total balance, nullable since it is initialized lazily the first time it is required in <see cref="UpdateState(DataFrame)"/>
    /// </summary>
    protected double? CachedTotalBalance { get; private set; }

    protected IExchange Exchange { get; private set; }

    /// <summary>
    /// Used leverage
    /// </summary>
    protected int Leverage { get; private set; }

    protected AbstractStrategy(StrategyConstructorParameters parameters)
    {
        Logger = parameters.Logger;
        Exchange = parameters.Exchange;
        MaxConcurrentPositions = Convert.ToInt32(parameters.Parameters.GetValueOrDefault("maxConcurrentPositions", 1));
        Leverage = Convert.ToInt32(parameters.Parameters.GetValueOrDefault("leverage", 1));
        shortConditions = new List<AbstractCondition>();
        longConditions = new List<AbstractCondition>();
    }

    /// <summary>
    ///   Reset the status of all conditions of the strategy
    /// </summary>
    public void Reset()
    {
        ResetState();
        indicators ??= GetIndicators();
        foreach (var i in indicators)
        {
            i.Reset();
        }
        foreach (var c in longConditions)
        {
            c.Reset();
        }
        foreach (var c in shortConditions)
        {
            c.Reset();
        }
    }

    /// <summary>
    ///   Update the internal state of the strategy
    /// </summary>
    /// <returns> </returns>
    public async Task UpdateState(DataFrame frame)
    {
        Exchange.Tick(frame);
        if (frame.IsClosed)
        {
            CachedTotalBalance = await Exchange.GetTotalBalanceAsync();
            thresholdBalance ??= CachedTotalBalance;
            if (ShouldWithdrawFromTradingAccount())
            {
                var amount = thresholdBalance * WithdrawRatio;
                await Exchange.WithdrawFromTradingBalanceAsync((double)amount);
                thresholdBalance = CachedTotalBalance * (WithdrawThreshold + 1);
            }
            await Tick(frame);
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
                var stake = GetMargin(frame);

                if (longConditions.Count > 0 && longConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Buy;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), (float)stake, Leverage);
                    longConditions.ForEach(c => c.Reset());
                }

                if (shortConditions.Count > 0 && shortConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Sell;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), (float)stake, Leverage);
                    shortConditions.ForEach(c => c.Reset());
                }
            }
        }
    }

    /// <summary>
    ///   Add short <see cref="AbstractCondition"/>
    /// </summary>
    protected void InjectShortConditions(params AbstractCondition[] conditions)
    {
        foreach (var c in conditions)
        {
            if (c is not null && !shortConditions.Contains(c))
            {
                shortConditions.Add(c);
            }
        }
    }

    /// <summary>
    ///   Reset the internal state of the strategy <i> (nullables, cached values ...) </i>
    /// </summary>
    protected abstract void ResetState();

    /// <summary>
    ///   Return the indicators of the strategy, so that they can be automatically reset in <see cref="Reset"/>
    /// </summary>
    /// <returns> </returns>
    protected abstract IEnumerable<Indicator> GetIndicators();

    /// <summary>
    ///   How much to invest in each trade
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetMargin(DataFrame frame);

    /// <summary>
    ///   Get the value for the stop loss for a given side order
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetStopLoss(OrderSide side, DataFrame frame);

    /// <summary>
    ///   Get the value for the take profit for a given side order
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetTakeProfit(OrderSide side, DataFrame frame);

    /// <summary>
    ///   Add long <see cref="AbstractCondition"/>
    /// </summary>
    protected void InjectLongConditions(params AbstractCondition[] conditions)
    {
        foreach (var c in conditions)
        {
            if (c is not null && !longConditions.Contains(c))
            {
                longConditions.Add(c);
            }
        }
    }

    /// <summary>
    ///   Called each time a closed candle is received
    /// </summary>
    /// <param name="frame"> </param>
    protected virtual Task Tick(DataFrame frame) => Task.CompletedTask;

    private bool ShouldWithdrawFromTradingAccount() =>
        WithdrawThreshold > 0 &&
        WithdrawRatio > 0 &&
        WithdrawThreshold >= WithdrawRatio &&
        CachedTotalBalance / thresholdBalance >= WithdrawThreshold + 1;
}