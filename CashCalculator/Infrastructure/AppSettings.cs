using System.Collections.Generic;

namespace CashCalculator.Infrastructure
{
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
}