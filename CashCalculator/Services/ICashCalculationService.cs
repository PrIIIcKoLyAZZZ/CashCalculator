using System.Collections.Generic;
using CashCalculator.Models;
using CashCalculator.Services.Calculation;

namespace CashCalculator.Services
{
    /// <summary>
    /// Defines methods for performing cash register calculations:
    /// - calculating the total sum
    /// - computing the difference (total − expected)
    /// - determining the status (OK/Under/Over)
    /// - producing a full summary result
    /// </summary>
    public interface ICashCalculationService
    {
        /// <summary>
        /// Calculates the total sum of all denominations.
        /// </summary>
        /// <param name="denoms">A collection of <see cref="Denomination"/> objects.</param>
        /// <returns>The sum of <c>Value × Amount</c> for each denomination.</returns>
        int CalculateTotal(IEnumerable<Denomination> denoms);

        /// <summary>
        /// Calculates the difference between the actual total and the expected amount.
        /// </summary>
        /// <param name="totalSum">The actual total sum in the register.</param>
        /// <param name="expectedSum">The expected sum to compare against.</param>
        /// <returns><c>totalSum − expectedSum</c>.</returns>
        int CalculateDifference(int totalSum, int expectedSum);

        /// <summary>
        /// Determines the status based on the difference:
        /// OK if zero, Under if negative, Over if positive.
        /// </summary>
        /// <param name="difference">The difference between actual and expected sums.</param>
        /// <returns>A <see cref="SummaryStatus"/> value.</returns>
        SummaryStatus GetStatus(int difference);

        /// <summary>
        /// Performs a full summary calculation, returning totals, expected value,
        /// difference, and status. Treats empty or invalid expected input as zero.
        /// </summary>
        /// <param name="denoms">A collection of <see cref="Denomination"/> objects.</param>
        /// <param name="expectedValueText">The expected amount as entered by the user.</param>
        /// <returns>A <see cref="SummaryResult"/> containing all computed fields.</returns>
        SummaryResult CalculateSummary(IEnumerable<Denomination> denoms, string expectedValueText);
    }
}