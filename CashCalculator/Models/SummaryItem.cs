using System.ComponentModel;
using System.Windows.Media;

namespace CashCalculator.Models
{
    /// <summary>
    /// Indicates the status of a summary item comparison.
    /// </summary>
    public enum SummaryStatus
    {
        /// <summary>No status.</summary>
        None,

        /// <summary>The value matches the expected target.</summary>
        OK,

        /// <summary>The value is below the expected target.</summary>
        Under,

        /// <summary>The value is above the expected target.</summary>
        Over
    }

    /// <summary>
    /// Represents a single summary entry with description, value, and status,
    /// providing UI notifications and visual indicators (glyph and brush).
    /// </summary>
    public class SummaryItem : INotifyPropertyChanged
    {
        private string _value;
        private SummaryStatus _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryItem"/> class.
        /// </summary>
        /// <param name="desc">The text description of this summary entry.</param>
        /// <param name="value">The string representation of the value to display.</param>
        /// <param name="status">
        /// The <see cref="SummaryStatus"/> indicating comparison result.
        /// Defaults to <see cref="SummaryStatus.None"/>.
        /// </param>
        public SummaryItem(string desc, string value, SummaryStatus status = SummaryStatus.None)
        {
            Description = desc;
            _value = value;
            _status = status;
        }

        /// <summary>
        /// Gets the description text for this summary entry.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets the displayed value for this entry.
        /// Raises <see cref="PropertyChanged"/> when changed.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        /// <summary>
        /// Gets or sets the status of this summary entry.
        /// Raises <see cref="PropertyChanged"/> for <see cref="Status"/>,
        /// <see cref="StatusGlyph"/>, and <see cref="StatusBrush"/> when changed.
        /// </summary>
        public SummaryStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusGlyph)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusBrush)));
            }
        }

        /// <summary>
        /// Gets the Unicode glyph corresponding to the current <see cref="Status"/>.
        /// </summary>
        public string StatusGlyph => Status switch
        {
            SummaryStatus.OK    => "✓",
            SummaryStatus.Under => "✘",
            SummaryStatus.Over  => "+",
            _                   => ""
        };

        /// <summary>
        /// Gets the brush color corresponding to the current <see cref="Status"/>.
        /// </summary>
        public Brush StatusBrush => Status switch
        {
            SummaryStatus.OK    => Brushes.Green,
            SummaryStatus.Under => Brushes.Red,
            SummaryStatus.Over  => Brushes.Blue,
            _                   => Brushes.Transparent
        };

        /// <summary>
        /// Occurs when a property value changes, used to notify UI bindings.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}