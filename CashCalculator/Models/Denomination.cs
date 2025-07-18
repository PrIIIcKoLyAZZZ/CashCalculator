using System;

namespace CashCalculator.Models
{
    /// <summary>
    /// Represents a currency denomination with a specific face value and count,
    /// and provides the total amount for that denomination.
    /// </summary>
    public class Denomination
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Denomination"/> class
        /// with the specified face value and count.
        /// </summary>
        /// <param name="value">The face value of the denomination (must be greater than zero).</param>
        /// <param name="amount">The number of units of this denomination (cannot be negative).</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="value"/> is less than or equal to zero,
        /// or if <paramref name="amount"/> is negative.
        /// </exception>
        public Denomination(int value, int amount)
        {
            if (value <= 0)
                throw new ArgumentException("Denomination value must be greater than zero", nameof(value));
            if (amount < 0)
                throw new ArgumentException("Denomination amount cannot be negative", nameof(amount));

            Value = value;
            Amount = amount;
        }

        /// <summary>
        /// Gets or sets the face value of the denomination (in currency units).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the number of units of this denomination.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets the total amount for this denomination
        /// (calculated as <see cref="Value"/> × <see cref="Amount"/>).
        /// </summary>
        public int Total => Value * Amount;
    }
}