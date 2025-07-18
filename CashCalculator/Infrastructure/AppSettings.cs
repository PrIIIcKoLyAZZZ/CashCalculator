using System.Collections.Generic;

namespace CashCalculator.Infrastructure
{
    public class AppSettings
    {
        // Видимость купюр
        public List<DenominationFilterState> Filters { get; set; } = new();

        // Последний введённый «должно получиться»
        public int LastExpected { get; set; }

        // Последние количества купюр
        public List<DenominationState> Denominations { get; set; } = new();
    }

    public class DenominationFilterState
    {
        public int Value { get; set; }
        public bool IsVisible { get; set; }
    }

    public class DenominationState
    {
        public int Value  { get; set; }
        public int Amount { get; set; }
    }
}