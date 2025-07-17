using System.ComponentModel;
using System.Windows.Media;

namespace CashCalculator.Models
{
    public enum SummaryStatus { None, OK, Under, Over }

    public class SummaryItem : INotifyPropertyChanged
    {
        public SummaryItem(string desc, string value, SummaryStatus status = SummaryStatus.None)
        {
            Description = desc;
            _value = value;
            _status = status;
        }

        public string Description { get; }

        private string _value;
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

        private SummaryStatus _status;
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

        public string StatusGlyph => Status switch
        {
            SummaryStatus.OK    => "✓",
            SummaryStatus.Under => "✘",
            SummaryStatus.Over  => "+",
            _                   => ""
        };

        public Brush StatusBrush => Status switch
        {
            SummaryStatus.OK    => Brushes.Green,
            SummaryStatus.Under => Brushes.Red,
            SummaryStatus.Over  => Brushes.Blue,
            _                   => Brushes.Transparent
        };

        public event PropertyChangedEventHandler PropertyChanged;
    }
}