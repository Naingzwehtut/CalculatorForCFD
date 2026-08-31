namespace RiskRewardCalculator.Models;

/// <summary>
/// The price levels that define a single trade idea, independent of how big
/// the position is or how much money is at risk.
/// </summary>
public class TradeInput
{
    public TradeDirection Direction { get; set; } = TradeDirection.Long;

    public decimal EntryPrice { get; set; }

    public decimal StopLossPrice { get; set; }

    public decimal TakeProfitPrice { get; set; }

    /// <summary>
    /// Distance between entry and stop loss, always returned as a positive number
    /// regardless of direction.
    /// </summary>
    public decimal StopLossDistance => Math.Abs(EntryPrice - StopLossPrice);

    /// <summary>
    /// Distance between entry and take profit, always returned as a positive number
    /// regardless of direction.
    /// </summary>
    public decimal TakeProfitDistance => Math.Abs(TakeProfitPrice - EntryPrice);

    /// <summary>
    /// Checks that stop loss and take profit sit on the correct side of entry
    /// for the chosen direction. Does not check for &gt; 0 - that is a separate,
    /// simpler rule enforced by numeric validation.
    /// </summary>
    public bool HasValidStopTakeProfitRelationship()
    {
        return Direction switch
        {
            TradeDirection.Long => StopLossPrice < EntryPrice && TakeProfitPrice > EntryPrice,
            TradeDirection.Short => StopLossPrice > EntryPrice && TakeProfitPrice < EntryPrice,
            _ => false
        };
    }
}
