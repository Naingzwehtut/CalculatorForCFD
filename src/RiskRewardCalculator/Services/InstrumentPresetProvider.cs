using RiskRewardCalculator.Models;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Supplies a starting-point set of contract parameters for a handful of common
/// instruments, purely so the user doesn't start from a blank form.
/// </summary>
/// <remarks>
/// These numbers are illustrative examples based on typical retail broker
/// conventions - they are <b>not</b> guaranteed to match any specific broker.
/// Contract size, point size, and point value genuinely vary between brokers,
/// account types (standard vs. cent/mini), and even between symbols that look
/// identical (e.g. "XAUUSD" vs "GOLD"). Always double check the values against
/// your own broker's contract specification page and edit them here before
/// trusting a real position size. Every field returned by this provider is
/// fully editable in the UI.
/// </remarks>
public class InstrumentPresetProvider
{
    public IReadOnlyList<InstrumentSettings> GetPresets() => new List<InstrumentSettings>
    {
        new()
        {
            Name = "XAU/USD (Gold)",
            PointSize = 0.01m,
            PointValuePerLot = 1m,     // 1 lot = 100 oz -> $1 per $0.01 move per oz * 100 oz = $1 per point
            ContractSize = 100m,
            PriceDecimals = 2
        },
        new()
        {
            Name = "EUR/USD",
            PointSize = 0.0001m,
            PointValuePerLot = 10m,    // standard lot, ~$10 per pip when quote currency is USD
            ContractSize = 100_000m,
            PriceDecimals = 5
        },
        new()
        {
            Name = "GBP/USD",
            PointSize = 0.0001m,
            PointValuePerLot = 10m,
            ContractSize = 100_000m,
            PriceDecimals = 5
        },
        new()
        {
            Name = "USD/JPY",
            PointSize = 0.01m,
            PointValuePerLot = 9.30m,  // approximate - depends on live USD/JPY rate, edit as needed
            ContractSize = 100_000m,
            PriceDecimals = 3
        },
        new()
        {
            Name = "BTC/USD",
            PointSize = 1m,
            PointValuePerLot = 1m,     // 1 "lot" = 1 BTC -> $1 per $1 move
            ContractSize = 1m,
            PriceDecimals = 2
        },
        InstrumentSettings.CreateCustomDefault()
    };
}
