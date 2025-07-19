using System;
using System.Collections.Generic;
using System.Linq;
using CashCalculator.Models;
using CashCalculator.Services.Interfaces;

namespace CashCalculator.Services.Calculation
{
    /// <summary>
    /// Provides methods to calculate totals, differences, and statuses for a cash register.
    /// </summary>
    public class CashCalculationService : ICashCalculationService
    {
        /// <summary>
        /// Calculates the sum of all denominations.
        /// </summary>
        /// <param name="denoms">A collection of <see cref="Denomination"/> objects.</param>
        /// <returns>The total sum computed as the sum of each denomination's <c>Total</c> property.</returns>
        public int CalculateTotal(IEnumerable<Denomination> denoms)
            => denoms.Sum(d => d.Total);

        /// <summary>
        /// Calculates the difference between the actual total and the expected amount.
        /// </summary>
        /// <param name="totalSum">The actual total sum in the register.</param>
        /// <param name="expectedSum">The expected sum to compare against.</param>
        /// <returns><c>totalSum − expectedSum</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expectedSum"/> is negative.</exception>
        public int CalculateDifference(int totalSum, int expectedSum)
        {
            if (expectedSum < 0)
                throw new ArgumentException("Expected sum cannot be negative", nameof(expectedSum));

            return totalSum - expectedSum;
        }

        /// <summary>
        /// Determines the status of the register based on the difference.
        /// </summary>
        /// <param name="difference">The difference between actual and expected sums.</param>
        /// <returns>
        /// <see cref="SummaryStatus.OK"/> if the difference is zero;
        /// <see cref="SummaryStatus.Under"/> if negative;
        /// <see cref="SummaryStatus.Over"/> if positive.
        /// </returns>
        public SummaryStatus GetStatus(int difference)
            => difference switch
            {
                0   => SummaryStatus.OK,
                < 0 => SummaryStatus.Under,
                > 0 => SummaryStatus.Over
            };

        /// <summary>
        /// Performs a full summary calculation: total, expected, difference, and status.
        /// </summary>
        /// <param name="denoms">A collection of <see cref="Denomination"/> objects.</param>
        /// <param name="expectedValueText">
        /// The user-entered expected value as a string. Non-numeric or empty input is treated as zero.
        /// </param>
        /// <returns>A <see cref="SummaryResult"/> containing all computed values.</returns>
        public SummaryResult CalculateSummary(IEnumerable<Denomination> denoms, string expectedValueText)
        {
            int total = CalculateTotal(denoms);

            bool valid = int.TryParse(expectedValueText, out int expected) && expected >= 0;
            if (!valid)
                expected = 0;

            int diff = CalculateDifference(total, expected);
            SummaryStatus status = GetStatus(diff);

            return new SummaryResult
            {
                TotalSum   = total,
                Expected   = expected,
                Difference = diff,
                Status     = status
            };
        }
    }
}