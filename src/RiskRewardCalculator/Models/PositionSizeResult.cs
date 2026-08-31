namespace RiskRewardCalculator.Models;

/// <summary>
/// Everything the Position Size Calculator produces from a <see cref="TradeInput"/>,
/// <see cref="RiskSettings"/> and <see cref="InstrumentSettings"/>.
/// </summary>
public class PositionSizeResult
{
    public bool IsValid => ValidationErrors.Count == 0;

    public List<string> ValidationErrors { get; init; } = new();

    public decimal RiskAmount { get; set; }

    public decimal RiskPercentOfAccount { get; set; }

    public decimal StopLossDistance { get; set; }

    public decimal TakeProfitDistance { get; set; }

    /// <summary>Position size expressed in lots/contracts (can be fractional).</summary>
    public decimal PositionSizeInLots { get; set; }

    /// <summary>Position size expressed in base units (lots * contract size).</summary>
    public decimal PositionSizeInUnits { get; set; }

    public decimal PotentialLoss { get; set; }

    public decimal PotentialProfit { get; set; }

    /// <summary>Reward divided by risk, e.g. 3 means a 1:3 trade.</summary>
    public decimal RiskRewardRatio { get; set; }

    /// <summary>
    /// The win rate (0-100) at which this trade's average outcome breaks even,
    /// given its risk/reward ratio: Risk / (Risk + Reward).
    /// </summary>
    public decimal BreakEvenWinRatePercent { get; set; }

    /// <summary>
    /// Approximate margin required to open the position at the configured leverage.
    /// This is informational only - actual broker margin rules can differ.
    /// </summary>
    public decimal EstimatedMarginRequired { get; set; }

    public static PositionSizeResult Invalid(IEnumerable<string> errors) => new()
    {
        ValidationErrors = errors.ToList()
    };
}
