using RiskRewardCalculator.Models;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Evaluates the risk/reward profile of a trade for a position size that is
/// already decided (as opposed to <see cref="IPositionSizeCalculator"/>, which
/// works out the position size for you).
/// </summary>
public interface IRiskRewardCalculator
{
    /// <param name="positionSizeInUnits">
    /// Number of base units held (e.g. shares, coins, or contracts). This calculator
    /// treats money-per-unit-of-price-move as exactly 1 per unit, which fits stocks,
    /// crypto, and any instrument quoted directly in its own currency. For leveraged
    /// FX/CFD instruments with a separate point value, use the Position Size
    /// Calculator instead, which is contract-spec aware.
    /// </param>
    RiskRewardResult Calculate(TradeInput trade, decimal positionSizeInUnits);
}
