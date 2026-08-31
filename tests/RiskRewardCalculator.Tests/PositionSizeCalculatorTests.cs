using RiskRewardCalculator.Models;
using RiskRewardCalculator.Services;
using Xunit;

namespace RiskRewardCalculator.Tests;

public class PositionSizeCalculatorTests
{
    private readonly PositionSizeCalculator _calculator = new();

    private static InstrumentSettings DefaultInstrument() => new()
    {
        Name = "Test",
        PointSize = 1m,          // 1 point = 1 price unit, so distances map 1:1 to "points"
        PointValuePerLot = 10m,  // $10 per point per lot
        ContractSize = 1_000m,
        PriceDecimals = 2
    };

    [Fact]
    public void LongTrade_WithOnePercentRisk_ProducesExpectedRiskAmountAndRatio()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(100m, result.RiskAmount);          // 1% of 10,000
        Assert.Equal(10m, result.StopLossDistance);
        Assert.Equal(30m, result.TakeProfitDistance);
        Assert.Equal(3m, result.RiskRewardRatio);        // 30 / 10
        Assert.Equal(100m, result.PotentialLoss);        // should equal the risk amount by construction
        Assert.Equal(300m, result.PotentialProfit);
    }

    [Fact]
    public void ShortTrade_ComputesDistancesAsPositiveNumbers()
    {
        var trade = new TradeInput { Direction = TradeDirection.Short, EntryPrice = 2500, StopLossPrice = 2510, TakeProfitPrice = 2470 };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(10m, result.StopLossDistance);
        Assert.Equal(30m, result.TakeProfitDistance);
        Assert.Equal(3m, result.RiskRewardRatio);
    }

    [Theory]
    [InlineData(2490, 2510, 1)]   // SL 10 away, TP 10 away -> 1:1
    [InlineData(2490, 2520, 2)]   // SL 10 away, TP 20 away -> 1:2
    [InlineData(2490, 2530, 3)]   // SL 10 away, TP 30 away -> 1:3
    public void LongTrade_RiskRewardRatio_MatchesDistanceRatio(decimal sl, decimal tp, decimal expectedRatio)
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = sl, TakeProfitPrice = tp };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(expectedRatio, result.RiskRewardRatio);
    }

    [Fact]
    public void LongTrade_WithStopLossAboveEntry_IsInvalid()
    {
        // Invalid SL for a Long trade: stop loss must be BELOW entry.
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2510, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Long", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LongTrade_WithTakeProfitBelowEntry_IsInvalid()
    {
        // Invalid TP for a Long trade: take profit must be ABOVE entry.
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2480 };
        var risk = new RiskSettings { AccountBalance = 10_000, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ShortTrade_WithInvalidStopAndTakeProfit_IsInvalid()
    {
        // For Short, SL must be above entry and TP must be below entry - this has both backwards.
        var trade = new TradeInput { Direction = TradeDirection.Short, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ZeroPercentRisk_IsInvalid()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 0 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.False(result.IsValid);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Risk %"));
    }

    [Fact]
    public void LargeAccount_ScalesPositionSizeLinearly()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(100_000m, result.RiskAmount);
    }

    [Fact]
    public void SmallAccount_StillCalculatesAFractionalPosition()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 50, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(0.5m, result.RiskAmount);
        Assert.True(result.PositionSizeInLots > 0);
    }

    [Fact]
    public void FixedRiskAmountMode_UsesAmountDirectlyAndComputesEquivalentPercent()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.FixedAmount, RiskAmount = 50 };

        var result = _calculator.Calculate(trade, risk, DefaultInstrument());

        Assert.True(result.IsValid);
        Assert.Equal(50m, result.RiskAmount);
        Assert.Equal(0.5m, result.RiskPercentOfAccount); // 50 / 10,000 * 100
    }

    [Fact]
    public void DifferentInstrumentSettings_ProduceDifferentPositionSizesForSameRisk()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, Mode = RiskMode.PercentOfAccount, RiskPercent = 1 };

        var goldLikeInstrument = new InstrumentSettings { PointSize = 0.01m, PointValuePerLot = 1m, ContractSize = 100m };
        var fxLikeInstrument = new InstrumentSettings { PointSize = 0.0001m, PointValuePerLot = 10m, ContractSize = 100_000m };

        var goldResult = _calculator.Calculate(trade, risk, goldLikeInstrument);
        var fxResult = _calculator.Calculate(trade, risk, fxLikeInstrument);

        Assert.True(goldResult.IsValid);
        Assert.True(fxResult.IsValid);
        Assert.NotEqual(goldResult.PositionSizeInLots, fxResult.PositionSizeInLots);
        // Risk amount and R:R should be identical regardless of instrument, since those
        // only depend on the trade prices and account risk, not the contract spec.
        Assert.Equal(goldResult.RiskAmount, fxResult.RiskAmount);
        Assert.Equal(goldResult.RiskRewardRatio, fxResult.RiskRewardRatio);
    }

    [Fact]
    public void InvalidContractSize_IsRejected()
    {
        var trade = new TradeInput { Direction = TradeDirection.Long, EntryPrice = 2500, StopLossPrice = 2490, TakeProfitPrice = 2530 };
        var risk = new RiskSettings { AccountBalance = 10_000, RiskPercent = 1 };
        var instrument = DefaultInstrument();
        instrument.ContractSize = 0;

        var result = _calculator.Calculate(trade, risk, instrument);

        Assert.False(result.IsValid);
    }
}
