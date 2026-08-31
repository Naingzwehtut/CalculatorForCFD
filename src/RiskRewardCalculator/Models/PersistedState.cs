namespace RiskRewardCalculator.Models;

/// <summary>
/// The subset of app state that gets remembered in the browser's local storage
/// between visits (see <see cref="Services.LocalStorageService"/>).
/// Deliberately a flat, simple DTO so it serializes/deserializes predictably.
/// </summary>
public class PersistedState
{
    public decimal AccountBalance { get; set; } = 10_000m;
    public RiskMode RiskMode { get; set; } = RiskMode.PercentOfAccount;
    public decimal RiskPercent { get; set; } = 1m;
    public decimal RiskAmount { get; set; } = 100m;
    public decimal Leverage { get; set; } = 1m;

    public string InstrumentName { get; set; } = "Custom";
    public decimal PointSize { get; set; } = 0.0001m;
    public decimal PointValuePerLot { get; set; } = 10m;
    public decimal ContractSize { get; set; } = 100_000m;
    public int PriceDecimals { get; set; } = 5;

    public TradeDirection Direction { get; set; } = TradeDirection.Long;
    public decimal EntryPrice { get; set; }
    public decimal StopLossPrice { get; set; }
    public decimal TakeProfitPrice { get; set; }
}
