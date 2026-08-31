using RiskRewardCalculator.Models;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Default implementation of <see cref="IPositionSizeCalculator"/>.
/// </summary>
/// <remarks>
/// Core formula, in plain terms:
/// <code>
/// moneyPerLotAtStop = (StopLossDistance / PointSize) * PointValuePerLot
/// positionSizeInLots = RiskAmount / moneyPerLotAtStop
/// </code>
/// "moneyPerLotAtStop" is how much one single lot would lose if price travelled
/// all the way from entry to the stop loss. Dividing the money you're willing to
/// lose by that number tells you how many lots you can safely hold.
/// No intermediate rounding is performed - only the final <see cref="PositionSizeResult"/>
/// fields are rounded, and only for display, by the UI layer.
/// </remarks>
public class PositionSizeCalculator : IPositionSizeCalculator
{
    public PositionSizeResult Calculate(TradeInput trade, RiskSettings risk, InstrumentSettings instrument)
    {
        var errors = Validate(trade, risk, instrument);
        if (errors.Count > 0)
        {
            return PositionSizeResult.Invalid(errors);
        }

        var stopLossDistance = trade.StopLossDistance;
        var takeProfitDistance = trade.TakeProfitDistance;

        var riskAmount = risk.ResolveRiskAmount();
        var riskPercent = risk.ResolveRiskPercent();

        // How much ONE lot would gain/lose per unit of price distance.
        var moneyPerPriceUnitPerLot = instrument.PointValuePerLot / instrument.PointSize;

        var moneyPerLotAtStop = stopLossDistance * moneyPerPriceUnitPerLot;

        // moneyPerLotAtStop is guaranteed > 0 here because validation already
        // rejected a zero stop-loss distance and zero point value/size.
        var positionSizeInLots = riskAmount / moneyPerLotAtStop;
        var positionSizeInUnits = positionSizeInLots * instrument.ContractSize;

        var potentialLoss = moneyPerLotAtStop * positionSizeInLots;
        var potentialProfit = takeProfitDistance * moneyPerPriceUnitPerLot * positionSizeInLots;

        var riskRewardRatio = stopLossDistance == 0 ? 0 : takeProfitDistance / stopLossDistance;

        var breakEvenWinRate = (potentialLoss + potentialProfit) == 0
            ? 0
            : potentialLoss / (potentialLoss + potentialProfit) * 100m;

        // Rough margin estimate: notional value of the position divided by leverage.
        // Real broker margin models (tiered margin, currency conversion, etc.) can differ.
        var notionalValue = positionSizeInUnits * trade.EntryPrice;
        var estimatedMargin = risk.Leverage <= 0 ? notionalValue : notionalValue / risk.Leverage;

        return new PositionSizeResult
        {
            RiskAmount = riskAmount,
            RiskPercentOfAccount = riskPercent,
            StopLossDistance = stopLossDistance,
            TakeProfitDistance = takeProfitDistance,
            PositionSizeInLots = positionSizeInLots,
            PositionSizeInUnits = positionSizeInUnits,
            PotentialLoss = potentialLoss,
            PotentialProfit = potentialProfit,
            RiskRewardRatio = riskRewardRatio,
            BreakEvenWinRatePercent = breakEvenWinRate,
            EstimatedMarginRequired = estimatedMargin
        };
    }

    private static List<string> Validate(TradeInput trade, RiskSettings risk, InstrumentSettings instrument)
    {
        var errors = new List<string>();

        if (risk.AccountBalance <= 0)
            errors.Add("Account balance must be greater than 0.");

        if (risk.Mode == RiskMode.PercentOfAccount)
        {
            if (risk.RiskPercent <= 0)
                errors.Add("Risk % must be greater than 0.");
            else if (risk.RiskPercent > 100)
                errors.Add("Risk % cannot exceed 100%.");
        }
        else
        {
            if (risk.RiskAmount <= 0)
                errors.Add("Risk amount must be greater than 0.");
            else if (risk.AccountBalance > 0 && risk.RiskAmount > risk.AccountBalance)
                errors.Add("Risk amount cannot exceed the account balance.");
        }

        if (trade.EntryPrice <= 0)
            errors.Add("Entry price must be greater than 0.");

        if (trade.StopLossPrice <= 0)
            errors.Add("Stop loss price must be greater than 0.");

        if (trade.TakeProfitPrice <= 0)
            errors.Add("Take profit price must be greater than 0.");

        if (trade.EntryPrice > 0 && trade.StopLossPrice > 0 && trade.TakeProfitPrice > 0
            && !trade.HasValidStopTakeProfitRelationship())
        {
            errors.Add(trade.Direction == TradeDirection.Long
                ? "For a Long trade, stop loss must be below entry and take profit must be above entry."
                : "For a Short trade, stop loss must be above entry and take profit must be below entry.");
        }

        if (instrument.ContractSize <= 0)
            errors.Add("Contract size must be greater than 0.");

        if (instrument.PointValuePerLot <= 0)
            errors.Add("Point/pip value must be greater than 0.");

        if (instrument.PointSize <= 0)
            errors.Add("Point/pip size must be greater than 0.");

        if (risk.Leverage < 0)
            errors.Add("Leverage cannot be negative.");

        return errors;
    }
}
