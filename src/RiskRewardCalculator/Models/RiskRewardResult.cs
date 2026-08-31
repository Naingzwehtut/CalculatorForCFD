namespace RiskRewardCalculator.Models;

/// <summary>
/// Everything the Risk &amp; Reward Calculator produces from a <see cref="TradeInput"/>
/// and a fixed position size.
/// </summary>
public class RiskRewardResult
{
    public bool IsValid => ValidationErrors.Count == 0;

    public List<string> ValidationErrors { get; init; } = new();

    public decimal StopLossDistance { get; set; }

    public decimal TakeProfitDistance { get; set; }

    /// <summary>Reward divided by risk, e.g. 3 means a 1:3 trade.</summary>
    public decimal RiskRewardRatio { get; set; }

    public decimal PotentialLoss { get; set; }

    public decimal PotentialProfit { get; set; }

    /// <summary>
    /// The win rate (0-100) at which this trade's average outcome breaks even,
    /// given its risk/reward ratio: Risk / (Risk + Reward).
    /// </summary>
    public decimal BreakEvenWinRatePercent { get; set; }

    public static RiskRewardResult Invalid(IEnumerable<string> errors) => new()
    {
        ValidationErrors = errors.ToList()
    };
}
