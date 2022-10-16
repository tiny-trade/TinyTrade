using TinyTrade.Core.Exchanges;
using TinyTrade.Core.Exchanges.Offline;
using Xunit;

namespace TinyTrade.Tests;

public class OfflinePositionTests
{
    private const float Tolerance = 0.0001F;

    public static IEnumerable<object[]> WinData()
    {
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 2, 1, 0.2F, 0.4 };
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 5, 2, 0.2F, 2 };
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 5, 5, 0.2F, 5 };
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 10, 10, 0.2F, 20 };

        yield return new object[] { OrderSide.Sell, 10, 8, 11, 2, 1, 0.25F, 0.5 };
        yield return new object[] { OrderSide.Sell, 10, 8, 11, 5, 2, 0.25F, 2.5 };
        yield return new object[] { OrderSide.Sell, 10, 8, 11, 5, 5, 0.25F, 6.25 };
        yield return new object[] { OrderSide.Sell, 10, 8, 11, 10, 10, 0.25F, 25 };
    }

    public static IEnumerable<object[]> LoseData()
    {
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 2, 1, -0.1F, -0.2, false };
        yield return new object[] { OrderSide.Buy, 10, 12, 9, 5, 2, -0.1F, -1, false };

        yield return new object[] { OrderSide.Buy, 10, 12, 2, 5, 10, -0.1F, -5, true };

        yield return new object[] { OrderSide.Sell, 10, 8, 12.5, 2, 1, -0.2F, -0.4, false };
        yield return new object[] { OrderSide.Sell, 10, 8, 12.5, 5, 2, -0.2F, -2, false };

        yield return new object[] { OrderSide.Sell, 10, 8, 20, 5, 10, -0.1F, -5, true };
    }

    [Theory]
    [MemberData(nameof(WinData))]
    public void OfflinePositionWinTest(OrderSide side, float o, float tp, float sl, float m, int l, float exResultRatio, float exProfit)
    {
        var offlinePos = new OfflinePosition(side, o, tp, sl, m, l);
        Assert.Equal(offlinePos.CounterValue, m * l, Tolerance);
        Assert.Equal(offlinePos.LiquidationPrice, side == OrderSide.Buy ? o - (o / l) : o + (o / l), Tolerance);
        Assert.False(offlinePos.IsClosed);
        Assert.False(offlinePos.IsWon, "Won");
        Assert.False(offlinePos.Liquidated, "Liquidated");
        Assert.Equal(0, offlinePos.ResultRatio);
        Assert.Equal(0, offlinePos.NetProfit);
        offlinePos.TryClose(o);
        Assert.False(offlinePos.IsClosed, "Closed");
        offlinePos.TryClose(side == OrderSide.Buy ? tp + Tolerance : tp - Tolerance);
        Assert.True(offlinePos.IsClosed, "Closed");
        Assert.True(offlinePos.IsWon, "Not won");
        Assert.Equal(offlinePos.ResultRatio, exResultRatio, Tolerance);
        Assert.Equal(offlinePos.NetProfit, exProfit, Tolerance);
    }

    [Theory]
    [MemberData(nameof(LoseData))]
    public void OfflinePositionLoseTest(OrderSide side, float o, float tp, float sl, float m, int l, float exResultRatio, float exProfit, bool liquidated)
    {
        var offlinePos = new OfflinePosition(side, o, tp, sl, m, l);
        Assert.Equal(offlinePos.CounterValue, m * l, Tolerance);
        Assert.Equal(offlinePos.LiquidationPrice, side == OrderSide.Buy ? o - (o / l) : o + (o / l), Tolerance);
        Assert.False(offlinePos.IsClosed, "Closed");
        Assert.False(offlinePos.IsWon, "Won");
        Assert.False(offlinePos.Liquidated, "Liquidated");
        Assert.Equal(0, offlinePos.ResultRatio);
        Assert.Equal(0, offlinePos.NetProfit);
        offlinePos.TryClose(o);
        Assert.False(offlinePos.IsClosed);
        offlinePos.TryClose(side == OrderSide.Buy ? sl - Tolerance : sl + Tolerance);
        Assert.True(offlinePos.IsClosed, "Not Closed");
        Assert.False(offlinePos.IsWon, "Won");
        Assert.Equal(offlinePos.ResultRatio, exResultRatio, Tolerance);
        Assert.Equal(offlinePos.NetProfit, exProfit, Tolerance);
        Assert.Equal(liquidated, offlinePos.Liquidated);
    }
}