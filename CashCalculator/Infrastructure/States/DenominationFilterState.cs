namespace CashCalculator.Infrastructure.States;

/// <summary>
/// Represents the visibility state of a specific denomination in the table.
/// </summary>
public class DenominationFilterState
{
    /// <summary>
    /// The face value of the banknote.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Indicates whether this denomination row is visible.
    /// </summary>
    public bool IsVisible { get; set; }
}