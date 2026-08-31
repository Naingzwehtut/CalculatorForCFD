using RiskRewardCalculator.Models;
using RiskRewardCalculator.Services;
using Xunit;

namespace RiskRewardCalculator.Tests;

public class RiskRewardCalculatorTests
{
    private readonly RiskRewardCalculatorService _calculator = new();

    [Fact]
    public void LongTrade_1To3_ComputesExpectedRatioAndBreakEven()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 10);

        Assert.True(result.IsValid);
        Assert.Equal(10m, result.StopLossDistance);
        Assert.Equal(30m, result.TakeProfitDistance);
        Assert.Equal(3m, result.RiskRewardRatio);
        Assert.Equal(100m, result.PotentialLoss);   // 10 distance * 10 units
        Assert.Equal(300m, result.PotentialProfit); // 30 distance * 10 units
        // Break-even = Risk / (Risk + Reward) = 100 / 400 = 25%
        Assert.Equal(25m, result.BreakEvenWinRatePercent);
    }

    [Fact]
    public void ShortTrade_ComputesPositiveDistancesAndCorrectRatio()
    {
        var trade = new TradeInput { Direction = TradeDirection.Short, EntryPrice = 2500, StopLossPrice = 2510, TakeProfitPrice = 2470 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 5);

        Assert.True(result.IsValid);
        Assert.Equal(10m, result.StopLossDistance);
        Assert.Equal(30m, result.TakeProfitDistance);
        Assert.Equal(3m, result.RiskRewardRatio);
    }

    [Theory]
    [InlineData(2490, 2510, 1, 50)]   // 1:1 -> break-even 50%
    [InlineData(2490, 2520, 2, 33.33)] // 1:2 -> break-even ~33.33%
    [InlineData(2490, 2530, 3, 25)]    // 1:3 -> break-even 25%
    public void BreakEvenWinRate_MatchesRiskRewardRatio(decimal sl, decimal tp, decimal expectedRatio, decimal expectedBreakEven)
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = sl, TakeProfitPrice = tp };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 1);

        Assert.Equal(expectedRatio, result.RiskRewardRatio);
        Assert.Equal(expectedBreakEven, Math.Round(result.BreakEvenWinRatePercent, 2));
    }

    [Fact]
    public void InvalidStopLoss_ForLongTrade_IsRejected()
    {
        // SL above entry is invalid for a Long trade.
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2510, TakeProfitPrice = 2530 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 1);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public void InvalidTakeProfit_ForShortTrade_IsRejected()
    {
        // TP above entry is invalid for a Short trade.
        var trade = new TradeInput { Direction = TradeDirection.Short, EntryPrice = 2500, StopLossPrice = 2510, TakeProfitPrice = 2530 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 1);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ZeroPositionSize_IsRejected()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 0);

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Position size"));
    }

    [Fact]
    public void NegativeOrZeroPrices_AreRejected()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 0, StopLossPrice = 2490, TakeProfitPrice = 2530 };

        var result = _calculator.Calculate(trade, positionSizeInUnits: 1);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LargePositionSize_ScalesProfitAndLossLinearlyWithoutChangingRatio()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };

        var small = _calculator.Calculate(trade, positionSizeInUnits: 1);
        var large = _calculator.Calculate(trade, positionSizeInUnits: 1_000_000);

        Assert.Equal(small.RiskRewardRatio, large.RiskRewardRatio);
        Assert.Equal(small.BreakEvenWinRatePercent, large.BreakEvenWinRatePercent);
        Assert.Equal(small.PotentialLoss * 1_000_000, large.PotentialLoss);
    }
}
