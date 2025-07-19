using CashCalculator.Models;

namespace CashCalculator.Services.Calculation
{
    /// <summary>
    /// Data Transfer Object for summary calculation results:
    /// - TotalSum: the actual sum in the register
    /// - Expected: the expected sum (zero if input was invalid)
    /// - Difference: TotalSum minus Expected
    /// - Status: the comparison status (OK, Under, Over)
    /// </summary>
    public class SummaryResult
    {
        public int TotalSum   { get; set; }
        public int Expected   { get; set; }
        public int Difference { get; set; }
        public SummaryStatus Status     { get; set; }
    }
}