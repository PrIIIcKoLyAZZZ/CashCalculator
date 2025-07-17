using System;

namespace CashCalculator.Models
{
    public class Denomination
    {
        public Denomination(int value, int amount)
        {
            if (value <= 0)
                throw new ArgumentException("Denomination value must be greater than zero", nameof(value));
            if (amount < 0)
                throw new ArgumentException("Denomination amount cannot be negative", nameof(amount));

            Value = value;
            Amount = amount;
        }

        /// <summary>Номинал, ₽.</summary>
        public int Value { get; set; }

        /// <summary>Количество купюр/монет.</summary>
        public int Amount { get; set; }

        /// <summary>Общая сумма по этому номиналу: Value * Amount.</summary>
        public int Total => Value * Amount;
    }
}