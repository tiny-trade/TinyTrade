using TinyTrade.Core.Constructs;
using TinyTrade.Core.Strategy;
using Xunit;

namespace TinyTrade.Tests;

public class ConditionsTests
{
    [Fact]
    public void PerpetualConditionTest()
    {
        var frame = new DataFrame(0, 0, 0, 0, 0, 0, 0, false);
        var condition = new PerpetualCondition(f => frame.IsClosed);
        condition.Tick(frame);
        Assert.False(condition.IsSatisfied);
        frame.IsClosed = true;
        condition.Tick(frame);
        Assert.True(condition.IsSatisfied);
    }

    [Fact]
    public void EventConditionTest()
    {
        var close = 10;
        var isClosed = false;
        var condition = new EventCondition(f => isClosed && close > 10, null, 2);
        condition.Tick(null!);
        Assert.False(condition.IsSatisfied);
        isClosed = true;
        close = 11;
        condition.Tick(null!);
        Assert.True(condition.IsSatisfied);
        close = 10;
        isClosed = false;
        condition.Tick(null!);
        condition.Tick(null!);
        Assert.True(condition.IsSatisfied);
        condition.Tick(null!);
        Assert.False(condition.IsSatisfied);

        close = 10;
        isClosed = false;
        condition = new EventCondition(f => isClosed && close > 10, f => !isClosed, 2);
        condition.Tick(null!);
        Assert.False(condition.IsSatisfied);
        isClosed = true;
        close = 11;
        condition.Tick(null!);
        Assert.True(condition.IsSatisfied);
        close = 10;
        condition.Tick(null!);
        condition.Tick(null!);
        Assert.True(condition.IsSatisfied);
        condition.Tick(null!);
        Assert.False(condition.IsSatisfied);
        condition.Reset();

        isClosed = true;
        close = 11;
        condition.Tick(null!);
        Assert.True(condition.IsSatisfied);
        isClosed = false;
        condition.Tick(null!);
        Assert.False(condition.IsSatisfied);
    }
}