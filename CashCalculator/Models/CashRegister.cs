using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashCalculator.Models
{
    /// <summary>
    /// Represents a cash register containing various denominations of currency.
    /// Allows setting counts for each denomination, calculating totals and differences,
    /// and producing a formatted summary of the current cash contents.
    /// </summary>
    public class CashRegister
    {
        private readonly List<Denomination> _denoms;

        /// <summary>
        /// Initializes a new instance of the <see cref="CashRegister"/> class
        /// with all supported denominations initialized to zero count.
        /// </summary>
        public CashRegister()
        {
            _denoms = new List<Denomination>
            {
                new Denomination(5000, 0),
                new Denomination(2000, 0),
                new Denomination(1000, 0),
                new Denomination(500,  0),
                new Denomination(200,  0),
                new Denomination(100,  0),
                new Denomination(50,   0),
                new Denomination(10,   0),
                new Denomination(5,    0),
                new Denomination(2,    0),
                new Denomination(1,    0),
            };
        }

        /// <summary>
        /// Gets the collection of denominations currently in the register.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{Denomination}"/> of all supported denominations
        /// with their current counts.
        /// </returns>
        public IEnumerable<Denomination> GetDenominations() => _denoms;

        /// <summary>
        /// Sets the count for a specific denomination.
        /// </summary>
        /// <param name="value">The face value of the denomination (in ₽).</param>
        /// <param name="amount">The number of banknotes of that denomination.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="amount"/> is negative or if the specified
        /// denomination <paramref name="value"/> is not supported.
        /// </exception>
        public void SetCount(int value, int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            var d = _denoms.FirstOrDefault(x => x.Value == value)
                    ?? throw new ArgumentException($"Denomination {value}₽ not supported", nameof(value));

            d.Amount = amount;
        }

        /// <summary>
        /// Calculates the total sum of money currently in the register.
        /// </summary>
        /// <returns>The sum of (<see cref="Denomination.Value"/> × <see cref="Denomination.Amount"/>) across all denominations.</returns>
        public int TotalSum() => _denoms.Sum(d => d.Total);

        /// <summary>
        /// Calculates the difference between the current total in the register and a target sum.
        /// </summary>
        /// <param name="targetSum">The target sum to compare against.</param>
        /// <returns>
        /// The result of <c>TotalSum() - targetSum</c>.
        /// Positive values indicate surplus; negative values indicate a shortfall.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="targetSum"/> is negative.
        /// </exception>
        public int CalculateDifference(int targetSum)
        {
            if (targetSum < 0)
                throw new ArgumentException("Target sum cannot be negative", nameof(targetSum));

            return TotalSum() - targetSum;
        }

        /// <summary>
        /// Returns a formatted string summarizing the counts of each denomination
        /// and the overall total in the register.
        /// </summary>
        /// <returns>
        /// A multi-line string listing each non-zero denomination count and the total sum.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Cash register summary:");
            foreach (var d in _denoms)
            {
                if (d.Amount > 0)
                    sb.AppendLine($"{d.Value}₽ - {d.Amount}");
            }

            sb.Append($"Total: {TotalSum()} ₽");
            return sb.ToString();
        }
    }
}