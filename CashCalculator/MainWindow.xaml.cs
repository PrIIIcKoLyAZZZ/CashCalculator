using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CashCalculator.Infrastructure;
using CashCalculator.Infrastructure.Data;
using CashCalculator.Infrastructure.States;
using CashCalculator.Models;
using CashCalculator.Services;
using CashCalculator.Services.Interfaces;

namespace CashCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.
    /// Hosts the denominations table, summary table, and touch-friendly numpad popup.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICashCalculationService _calcService;
        private bool _isTouchMode;
        private string _buffer = string.Empty;
        private DataGridCell? _activeCell;

        /// <summary>
        /// Collection of current currency denominations in the register.
        /// </summary>
        public ObservableCollection<Denomination> Denominations { get; }

        /// <summary>
        /// Collection of summary items: [0]=Total, [1]=Expected, [2]=Difference.
        /// </summary>
        public ObservableCollection<SummaryItem> SummaryItems { get; }

        /// <summary>
        /// Collection of visibility filters for each denomination row in the UI.
        /// </summary>
        public ObservableCollection<DenominationFilter> DenominationFilters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// Populates UI from provided settings and calculation data.
        /// </summary>
        /// <param name="calcService">The service used to perform cash calculations.</param>
        /// <param name="settings">Previously loaded application settings (filters and saved counts).</param>
        /// <param name="calcData">Previously loaded calculation data (last totals and counts).</param>
        public MainWindow(
            ICashCalculationService calcService,
            AppSettings settings,
            CalculationData calcData)
        {
            InitializeComponent();

            _calcService = calcService;

            // 1) Initialize the denominations collection
            Denominations = new ObservableCollection<Denomination>(
                new CashRegister().GetDenominations()
            );

            // 2) Initialize filters before setting the collection source
            DenominationFilters = new ObservableCollection<DenominationFilter>(
                Denominations.Select(d =>
                {
                    var f = new DenominationFilter(d.Value, true);
                    f.PropertyChanged += (_, __) =>
                    {
                        var cvs = (CollectionViewSource)FindResource("DenomsViewSource");
                        cvs.View?.Refresh();
                    };
                    return f;
                })
            );

            // 3) Apply saved filter states
            foreach (var sf in settings.Filters)
            {
                var f = DenominationFilters.FirstOrDefault(x => x.Value == sf.Value);
                if (f != null)
                    f.IsVisible = sf.IsVisible;
            }

            // 4) Bind DataContext and configure the CollectionViewSource
            DataContext = this;
            var denomsViewSource = (CollectionViewSource)FindResource("DenomsViewSource");
            denomsViewSource.Filter += DenomsFilter;
            denomsViewSource.Source = Denominations;

            // 5) Initialize the summary items from calcData
            SummaryItems = new ObservableCollection<SummaryItem>
            {
                new SummaryItem("Total", $"{calcData.LastTotal} ₽", SummaryStatus.None),
                new SummaryItem("Expected Amount", calcData.LastExpected.ToString(), SummaryStatus.None),
                new SummaryItem("Difference", $"{calcData.LastDifference} ₽", SummaryStatus.None)
            };
            SummaryGrid.ItemsSource = SummaryItems;

            // 6) Apply saved denomination amounts from calcData
            foreach (var ds in calcData.Denominations)
            {
                var d = Denominations.FirstOrDefault(x => x.Value == ds.Value);
                if (d != null)
                    d.Amount = ds.Amount;
            }

            // 7) Perform initial totals calculation
            UpdateTotals();
        }

        #region Touch Mode and NumPad

        private void TouchModeButton_Checked(object _, RoutedEventArgs __)   => _isTouchMode = true;
        private void TouchModeButton_Unchecked(object _, RoutedEventArgs __) => _isTouchMode = false;

        private void DenomsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell) &&
                cell.Column.Header?.ToString() == "Кол-во")
            {
                e.Handled = true;
                var current = ((Denomination)cell.DataContext).Amount.ToString();
                OpenNumpad(cell, current == "0" ? string.Empty : current);
            }
        }

        private void SummaryGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell) &&
                cell.Column.DisplayIndex == 2 &&
                cell.DataContext is SummaryItem si &&
                si.Description.StartsWith("Expected"))
            {
                e.Handled = true;
                var val = si.Value.TrimEnd(' ', '₽');
                OpenNumpad(cell, val);
            }
        }

        private void OpenNumpad(DataGridCell cell, string initial)
        {
            _activeCell = cell;
            _buffer     = initial;
            NumpadDisplay.Text = initial;
            NumpadPopup.IsOpen = true;
        }

        private void Numpad_OnDigit(object sender, RoutedEventArgs _)
        {
            _buffer += ((Button)sender).Content;
            NumpadDisplay.Text = _buffer;
        }

        private void Numpad_OnBackspace(object _, RoutedEventArgs __)
        {
            if (_buffer.Length > 0)
                _buffer = _buffer[..^1];
            NumpadDisplay.Text = _buffer;
        }

        private void Numpad_OnEnter(object _, RoutedEventArgs __)
        {
            CommitBuffer();
            NumpadPopup.IsOpen = false;
        }

        private void CommitBuffer()
        {
            if (_activeCell == null) return;

            if (_activeCell.DataContext is Denomination d)
            {
                d.Amount = string.IsNullOrEmpty(_buffer) ? 0 : int.Parse(_buffer);
                DenomsGrid.Items.Refresh();
            }
            else if (_activeCell.DataContext is SummaryItem si)
            {
                si.Value = string.IsNullOrEmpty(_buffer) ? "0" : _buffer;
                SummaryGrid.Items.Refresh();
            }

            UpdateTotals();
            _buffer     = string.Empty;
            _activeCell = null;
        }

        #endregion

        #region Input Validation and Editing

        private static readonly Regex DigitsOnlyRegex = new("[^0-9]+");

        private void DataGrid_PreviewTextInput(object _, TextCompositionEventArgs e)
            => e.Handled = DigitsOnlyRegex.IsMatch(e.Text);

        private void DenomsGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Кол-во" &&
                e.EditingElement is TextBox tb &&
                e.Row.Item is Denomination d)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                    tb.Text = "0";
                d.Amount = int.Parse(tb.Text);
            }
            Dispatcher.InvokeAsync(UpdateTotals);
        }

        private void SummaryGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox tb &&
                e.Row.Item is SummaryItem si &&
                si.Description.StartsWith("Expected"))
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                    tb.Text = "0";
                si.Value = tb.Text;
            }
            Dispatcher.InvokeAsync(UpdateTotals);
        }

        #endregion

        #region Filtering

        private void DenomsFilter(object sender, FilterEventArgs e)
        {
            if (DenominationFilters == null)
            {
                e.Accepted = true;
                return;
            }

            if (e.Item is Denomination d)
            {
                var flt = DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        #endregion

        #region Totals Calculation

        private void UpdateTotals()
        {
            var total = _calcService.CalculateTotal(Denominations);
            SummaryItems[0].Value = $"{total} ₽";

            bool valid = int.TryParse(SummaryItems[1].Value, out var expected) && expected >= 0;
            if (!valid)
            {
                SummaryItems[2].Value = "—";
                SummaryItems[2].Status = SummaryStatus.None;
            }
            else
            {
                var diff = _calcService.CalculateDifference(total, expected);
                SummaryItems[2].Value = $"{diff} ₽";
                SummaryItems[2].Status = _calcService.GetStatus(diff);
            }
        }

        #endregion

        #region Copy and Clear Commands

        private void CopyReport_Click(object _, RoutedEventArgs __)
        {
            UpdateTotals();
            var register = new CashRegister();
            foreach (var d in Denominations)
                register.SetCount(d.Value, d.Amount);
            Clipboard.SetText(register.ToString());
        }

        private void Clean_Click(object _, RoutedEventArgs __)
        {
            foreach (var d in Denominations)
                d.Amount = 0;

            DenomsGrid.Items.Refresh();
            SummaryItems[1].Value = string.Empty;
            UpdateTotals();
        }

        #endregion

        #region Helpers

        private static bool TryFindParent<T>(DependencyObject start, out T parent)
            where T : DependencyObject
        {
            while (start != null)
            {
                if (start is T found)
                {
                    parent = found;
                    return true;
                }
                start = VisualTreeHelper.GetParent(start);
            }
            parent = null!;
            return false;
        }

        #endregion

        #region Settings & Data Serialization

        /// <summary>
        /// Собирает текущее состояние фильтров и номиналов для сохранения в AppSettings.
        /// </summary>
        public AppSettings GetAppSettings()
        {
            var cfg = new AppSettings
            {
                Filters = DenominationFilters
                    .Select(f => new DenominationFilterState { Value = f.Value, IsVisible = f.IsVisible })
                    .ToList(),
                LastExpected = int.TryParse(SummaryItems[1].Value, out var exp) && exp >= 0 ? exp : 0,
                Denominations = Denominations
                    .Select(d => new DenominationState { Value = d.Value, Amount = d.Amount })
                    .ToList()
            };
            var total = _calcService.CalculateTotal(Denominations);
            cfg.LastTotal = total;
            cfg.LastDifference = total - cfg.LastExpected;
            return cfg;
        }

        /// <summary>
        /// Собирает текущее состояние пересчёта для сохранения в CalculationData.
        /// </summary>
        public CalculationData GetCalculationData()
        {
            int expected = int.TryParse(SummaryItems[1].Value, out var exp) && exp >= 0 ? exp : 0;
            var total = _calcService.CalculateTotal(Denominations);
            var data = new CalculationData
            {
                LastExpected = expected,
                LastTotal = total,
                LastDifference = _calcService.CalculateDifference(total, expected),
                Denominations = Denominations
                    .Select(d => new DenominationState { Value = d.Value, Amount = d.Amount })
                    .ToList()
            };
            return data;
        }

        #endregion
    }
}