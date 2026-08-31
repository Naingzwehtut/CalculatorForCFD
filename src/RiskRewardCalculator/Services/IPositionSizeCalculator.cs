using RiskRewardCalculator.Models;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Works out how many lots/units to trade so that hitting the stop loss loses
/// exactly the amount of money the trader decided to risk.
/// </summary>
public interface IPositionSizeCalculator
{
    PositionSizeResult Calculate(TradeInput trade, RiskSettings risk, InstrumentSettings instrument);
}
