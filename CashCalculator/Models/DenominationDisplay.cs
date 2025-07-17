using System.ComponentModel;

namespace CashCalculator.Models
{
    public class DenominationDisplay : INotifyPropertyChanged
    {
        public DenominationDisplay(string name, int amount, bool isSum)
        {
            Name = name;
            _amount = amount;
            IsSum = isSum;
        }

        public string Name { get; }
        public bool IsSum { get; }

        private int _amount;
        public int Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}