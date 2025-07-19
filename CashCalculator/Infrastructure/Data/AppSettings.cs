using System.Collections.Generic;
using CashCalculator.Infrastructure.States;

namespace CashCalculator.Infrastructure.Data;
    /// <summary>
    /// Represents the application settings, which are fully serialized to a JSON file.
    /// </summary>
public class AppSettings 
{ 
    /// <summary>
    /// Visibility settings for each banknote denomination in the display table.
    /// </summary>
    public List<DenominationFilterState> Filters { get; set; } = new();

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