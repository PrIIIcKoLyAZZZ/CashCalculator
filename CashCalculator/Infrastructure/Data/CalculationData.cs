using System.Collections.Generic;
using CashCalculator.Infrastructure.States;

namespace CashCalculator.Infrastructure.Data;

/// <summary>
/// Последние данные пересчёта: целевая сумма, итог, разница и состояние купюр.
/// </summary>
public class CalculationData
{
    /// <summary>
    /// The last entered expected total amount ("target amount").
    /// </summary>
    public int LastExpected { get; set; }

    /// <summary>
    /// The sum of entered denominations at the time of the last save.
    /// </summary>
    public int LastTotal { get; set; }

    /// <summary>
    /// The difference (Total – Expected) at the time of the last save.
    /// </summary>
    public int LastDifference { get; set; }

    /// <summary>
    /// The last recorded counts for each denomination.
    /// </summary>
    public List<DenominationState> Denominations { get; set; } = new();
}