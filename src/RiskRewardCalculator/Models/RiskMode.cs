namespace RiskRewardCalculator.Models;

/// <summary>
/// How the trader wants to express how much they are willing to lose.
/// </summary>
public enum RiskMode
{
    /// <summary>Risk is entered as a percentage of the account balance (e.g. 1%).</summary>
    PercentOfAccount,

    /// <summary>Risk is entered directly as a currency amount (e.g. $50).</summary>
    FixedAmount
}
