namespace RiskRewardCalculator.Models;

/// <summary>
/// Describes how much of the account the trader is willing to risk on one trade.
/// </summary>
public class RiskSettings
{
    public decimal AccountBalance { get; set; } = 10_000m;

    public RiskMode Mode { get; set; } = RiskMode.PercentOfAccount;

    /// <summary>Used when <see cref="Mode"/> is <see cref="RiskMode.PercentOfAccount"/>.</summary>
    public decimal RiskPercent { get; set; } = 1m;

    /// <summary>Used when <see cref="Mode"/> is <see cref="RiskMode.FixedAmount"/>.</summary>
    public decimal RiskAmount { get; set; } = 100m;

    /// <summary>
    /// Optional leverage multiplier (e.g. 30 for 1:30). Only used to report the
    /// margin required to open the calculated position - it never changes the
    /// risk math itself, because risk is driven by the stop loss distance, not leverage.
    /// </summary>
    public decimal Leverage { get; set; } = 1m;

    /// <summary>
    /// Resolves the actual dollar risk amount for the current mode, given a balance.
    /// This is the single source of truth both calculators and the UI should use,
    /// so the two input modes can never silently disagree.
    /// </summary>
    public decimal ResolveRiskAmount() => Mode switch
    {
        RiskMode.PercentOfAccount => AccountBalance * (RiskPercent / 100m),
        RiskMode.FixedAmount => RiskAmount,
        _ => 0m
    };

    /// <summary>
    /// Resolves the risk expressed as a percentage of the account, regardless of
    /// which mode was used to enter it. Useful for display when the user typed a
    /// fixed dollar amount but still wants to see what % of the account that is.
    /// </summary>
    public decimal ResolveRiskPercent() => Mode switch
    {
        RiskMode.PercentOfAccount => RiskPercent,
        RiskMode.FixedAmount => AccountBalance == 0 ? 0 : (RiskAmount / AccountBalance) * 100m,
        _ => 0m
    };
}
