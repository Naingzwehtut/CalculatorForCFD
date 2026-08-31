using RiskRewardCalculator.Models;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Default implementation of <see cref="IRiskRewardCalculator"/>.
/// </summary>
/// <remarks>
/// Named <c>RiskRewardCalculatorService</c> (rather than <c>RiskRewardCalculator</c>)
/// purely to avoid clashing with the project's own root namespace, which is also
/// called <c>RiskRewardCalculator</c>.
/// <code>
/// RiskRewardRatio = TakeProfitDistance / StopLossDistance
/// BreakEvenWinRate = Risk / (Risk + Reward)
/// </code>
/// </remarks>
public class RiskRewardCalculatorService : IRiskRewardCalculator
{
    public RiskRewardResult Calculate(TradeInput trade, decimal positionSizeInUnits)
    {
        var errors = Validate(trade, positionSizeInUnits);
        if (errors.Count > 0)
        {
            return RiskRewardResult.Invalid(errors);
        }

        var stopLossDistance = trade.StopLossDistance;
        var takeProfitDistance = trade.TakeProfitDistance;

        var potentialLoss = stopLossDistance * positionSizeInUnits;
        var potentialProfit = takeProfitDistance * positionSizeInUnits;

        var riskRewardRatio = stopLossDistance == 0 ? 0 : takeProfitDistance / stopLossDistance;

        // Risk / (Risk + Reward). Using money amounts and using raw distances
        // gives the identical ratio, since positionSizeInUnits cancels out -
        // money is used here so the formula reads the same as it's usually taught.
        var breakEvenWinRate = (potentialLoss + potentialProfit) == 0
            ? 0
            : potentialLoss / (potentialLoss + potentialProfit) * 100m;

        return new RiskRewardResult
        {
            StopLossDistance = stopLossDistance,
            TakeProfitDistance = takeProfitDistance,
            RiskRewardRatio = riskRewardRatio,
            PotentialLoss = potentialLoss,
            PotentialProfit = potentialProfit,
            BreakEvenWinRatePercent = breakEvenWinRate
        };
    }

    private static List<string> Validate(TradeInput trade, decimal positionSizeInUnits)
    {
        var errors = new List<string>();

        if (trade.EntryPrice <= 0)
            errors.Add("Entry price must be greater than 0.");

        if (trade.StopLossPrice <= 0)
            errors.Add("Stop loss price must be greater than 0.");

        if (trade.TakeProfitPrice <= 0)
            errors.Add("Take profit price must be greater than 0.");

        if (positionSizeInUnits <= 0)
            errors.Add("Position size must be greater than 0.");

        if (trade.EntryPrice > 0 && trade.StopLossPrice > 0 && trade.TakeProfitPrice > 0
            && !trade.HasValidStopTakeProfitRelationship())
        {
            errors.Add(trade.Direction == TradeDirection.Long
                ? "For a Long trade, stop loss must be below entry and take profit must be above entry."
                : "For a Short trade, stop loss must be above entry and take profit must be below entry.");
        }

        return errors;
    }
}
