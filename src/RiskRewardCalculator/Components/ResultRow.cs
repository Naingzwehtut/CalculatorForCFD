namespace RiskRewardCalculator.Components;

/// <summary>A single label/value pair shown inside a <see cref="ResultsCard"/>.</summary>
/// <param name="Label">Short description, e.g. "Risk Amount".</param>
/// <param name="Value">Pre-formatted display string, e.g. "$100.00".</param>
/// <param name="Emphasize">When true, renders the value larger/bolder (for headline numbers).</param>
public record ResultRow(string Label, string Value, bool Emphasize = false);
