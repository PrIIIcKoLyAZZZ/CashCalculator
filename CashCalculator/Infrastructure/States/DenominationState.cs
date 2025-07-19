namespace CashCalculator.Infrastructure.States;

/// <summary>
/// Represents the count of a specific denomination.
/// </summary>
public class DenominationState
{
    /// <summary>
    /// The face value of the banknote.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// The quantity of banknotes of this denomination.
    /// </summary>
    public int Amount { get; set; }
}