using System.ComponentModel;

namespace CashCalculator.Models
{
    public class DenominationFilter : INotifyPropertyChanged
    {
        public DenominationFilter(int value, bool isVisible)
        {
            Value = value;
            _isVisible = isVisible;
        }

        /// <summary>Номинал, ₽.</summary>
        public int Value { get; }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}