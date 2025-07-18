using System.ComponentModel;

namespace CashCalculator.Models
{
    /// <summary>
    /// View model for displaying a denomination or total summary in the UI,
    /// with change notification support for the Amount property.
    /// </summary>
    public class DenominationDisplay : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenominationDisplay"/> class.
        /// </summary>
        /// <param name="name">The display name for the denomination or summary.</param>
        /// <param name="amount">The initial amount or total value to display.</param>
        /// <param name="isSum">
        /// A flag indicating whether this entry represents a sum/total row
        /// rather than an individual denomination.
        /// </param>
        public DenominationDisplay(string name, int amount, bool isSum)
        {
            Name = name;
            _amount = amount;
            IsSum = isSum;
        }

        /// <summary>
        /// Gets the display name of the denomination or summary row.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this row represents a sum/total.
        /// </summary>
        public bool IsSum { get; }

        private int _amount;

        /// <summary>
        /// Gets or sets the amount for this denomination or summary.
        /// Raises <see cref="PropertyChanged"/> when changed.
        /// </summary>
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

        /// <summary>
        /// Occurs when a property value changes, used to notify UI bindings.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}