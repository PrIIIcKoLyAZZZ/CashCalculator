using System;
using System.ComponentModel;

namespace CashCalculator.Models
{
    /// <summary>
    /// Represents a currency denomination with change notification.
    /// </summary>
    public class Denomination : INotifyPropertyChanged
    {
        public Denomination(int value, int amount)
        {
            if (value <= 0) throw new ArgumentException("Denomination value must be greater than zero", nameof(value));
            Value = value;
            _amount = amount;
        }

        /// <summary>
        /// Face value of the denomination.
        /// </summary>
        public int Value { get; set; }

        private int _amount;
        /// <summary>
        /// Number of units of this denomination.
        /// </summary>
        public int Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                OnPropertyChanged(nameof(Total));
            }
        }

        /// <summary>
        /// Total = Value × Amount.
        /// </summary>
        public int Total => Value * Amount;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}