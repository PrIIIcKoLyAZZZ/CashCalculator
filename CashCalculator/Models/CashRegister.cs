using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashCalculator.Models
{
    public class CashRegister
    {
        private readonly List<Denomination> _denoms;

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
                new Denomination(1,    0),
            };
        }

        public IEnumerable<Denomination> GetDenominations() => _denoms;

        public void SetCount(int value, int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            var d = _denoms.FirstOrDefault(x => x.Value == value)
                    ?? throw new ArgumentException($"Denomination {value}₽ not supported");
            d.Amount = amount;
        }

        public int TotalSum() => _denoms.Sum(d => d.Total);

        public int CalculateDifference(int targetSum)
        {
            if (targetSum < 0)
                throw new ArgumentException("Target sum cannot be negative", nameof(targetSum));

            return TotalSum() - targetSum;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var d in _denoms)
            {
                if (d.Amount > 0)
                    sb.AppendLine($"{d.Value}₽ - {d.Amount}");
            }

            sb.Append($"Сумма: {TotalSum()} ₽");
            return sb.ToString();
        }
    }
}