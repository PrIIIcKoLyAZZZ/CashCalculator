using System.ComponentModel;

namespace CashCalculator.Models
{
    /// <summary>
    /// View model representing the visibility filter for a specific currency denomination,
    /// with change notification support for the IsVisible property.
    /// </summary>
    public class DenominationFilter : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenominationFilter"/> class.
        /// </summary>
        /// <param name="value">The face value of the denomination (in currency units).</param>
        /// <param name="isVisible">
        /// Initial visibility state of this denomination in the UI.
        /// </param>
        public DenominationFilter(int value, bool isVisible)
        {
            Value = value;
            _isVisible = isVisible;
        }

        /// <summary>
        /// Gets the face value of the denomination (in currency units).
        /// </summary>
        public int Value { get; }

        private bool _isVisible;

        /// <summary>
        /// Gets or sets a value indicating whether this denomination is visible in the UI.
        /// Raises <see cref="PropertyChanged"/> when changed.
        /// </summary>
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

        /// <summary>
        /// Occurs when a property value changes, used to notify UI bindings.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}