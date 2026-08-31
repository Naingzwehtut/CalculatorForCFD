namespace RiskRewardCalculator.Models;

/// <summary>
/// Describes how a specific instrument converts a price move into money.
/// </summary>
/// <remarks>
/// IMPORTANT: these numbers are NOT universal constants. Every broker can define
/// contract size, point size, and point value differently for the same symbol
/// (e.g. XAU/USD on one broker might be quoted with a 1-lot = 100 oz contract and
/// a point size of 0.01, while another broker uses different conventions entirely).
/// Always confirm these values against your own broker's contract specification
/// sheet before using this calculator with real money. The presets in
/// <see cref="InstrumentPresetProvider"/> are illustrative starting points only,
/// not guaranteed-correct broker data.
/// </remarks>
public class InstrumentSettings
{
    /// <summary>Display name, e.g. "XAU/USD" or "Custom".</summary>
    public string Name { get; set; } = "Custom";

    /// <summary>
    /// The smallest price increment that the "Point/Pip Value" below is quoted for.
    /// For EUR/USD this is typically 0.0001 (a "pip"), for XAU/USD it might be 0.01,
    /// and for BTC/USD it might be 1.
    /// </summary>
    public decimal PointSize { get; set; } = 0.0001m;

    /// <summary>
    /// How much profit or loss ONE contract/lot makes for a move of exactly one
    /// <see cref="PointSize"/>. This is the number your broker publishes as
    /// "pip value" or "tick value" for one standard lot.
    /// </summary>
    public decimal PointValuePerLot { get; set; } = 10m;

    /// <summary>
    /// The number of base units represented by one "lot" (contract size),
    /// e.g. 100,000 units for a standard FX lot, 100 oz for a gold contract,
    /// or 1 coin for a BTC contract quoted directly in coins.
    /// </summary>
    public decimal ContractSize { get; set; } = 100_000m;

    /// <summary>Number of decimal places to display prices with for this instrument.</summary>
    public int PriceDecimals { get; set; } = 5;

    public static InstrumentSettings CreateCustomDefault() => new()
    {
        Name = "Custom",
        PointSize = 0.0001m,
        PointValuePerLot = 10m,
        ContractSize = 100_000m,
        PriceDecimals = 5
    };
}
