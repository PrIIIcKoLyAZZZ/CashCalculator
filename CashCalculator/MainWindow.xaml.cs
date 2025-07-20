using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CashCalculator.Infrastructure.Data;
using CashCalculator.Infrastructure.States;
using CashCalculator.Models;
using CashCalculator.Services.Interfaces;

namespace CashCalculator
{
    public partial class MainWindow : Window
    {
        private readonly ICashCalculationService _calcService;
        private bool   _isTouchMode;
        private string _buffer = string.Empty;
        private DataGridCell? _activeCell;

        public ObservableCollection<Denomination>        Denominations       { get; }
        public ObservableCollection<DenominationFilter>  DenominationFilters { get; }
        public ObservableCollection<SummaryItem>         SummaryItems        { get; }

        public MainWindow(ICashCalculationService calcService, AppSettings settings)
        {
            InitializeComponent();
            DataContext = this;

            _calcService = calcService;

            // 1) Kupюры + фильтры
            Denominations = new ObservableCollection<Denomination>(
                new CashRegister().GetDenominations()
            );
            DenominationFilters = new ObservableCollection<DenominationFilter>(
                Denominations.Select(d => new DenominationFilter(d.Value, true))
            );
            foreach (var sf in settings.Filters)
            {
                var f = DenominationFilters.FirstOrDefault(x => x.Value == sf.Value);
                if (f != null) f.IsVisible = sf.IsVisible;
            }

            // 2) SummaryItems
            SummaryItems = new ObservableCollection<SummaryItem>
            {
                new("Total",           $"{settings.LastTotal} ₽",        SummaryStatus.None),
                new("Expected Amount", settings.LastExpected.ToString(), SummaryStatus.None),
                new("Difference",      $"{settings.LastDifference} ₽",    SummaryStatus.None)
            };
            SummaryGrid.ItemsSource = SummaryItems;

            // 3) Восстановим сохранённые количества купюр
            foreach (var ds in settings.Denominations)
            {
                var d = Denominations.FirstOrDefault(x => x.Value == ds.Value);
                if (d != null) d.Amount = ds.Amount;
            }

            // 4) Toolbar события
            ToolbarView.CopyRequested    += (_,__)=> CopyReport_Click(null, null);
            ToolbarView.ClearRequested   += (_,__)=> Clean_Click     (null, null);
            ToolbarView.TouchModeToggled += (_,on)=> _isTouchMode = on;

            // 5) Numpad события
            // При нажатии цифры — только меняем буфер, без Show()
            NumpadView.DigitPressed     += (_, c)=> { _buffer += c; };
            // При Backspace — изменяем буфер и обновляем текст
            NumpadView.BackspacePressed += (_,__)=>
            {
                if (_buffer.Length > 0) _buffer = _buffer[..^1];
                NumpadView.Show(_buffer);
            };
            // При Enter — сохраняем и обновляем таблицы
            NumpadView.EnterPressed     += (_,__)=>
            {
                if (_activeCell?.DataContext is Denomination d)
                {
                    d.Amount = string.IsNullOrEmpty(_buffer) ? 0 : int.Parse(_buffer);
                    DenomsGrid.Items.Refresh();
                }
                else if (_activeCell?.DataContext is SummaryItem si)
                {
                    si.Value = string.IsNullOrEmpty(_buffer) ? "0" : _buffer;
                    SummaryGrid.Items.Refresh();
                }

                _buffer     = string.Empty;
                _activeCell = null;
                UpdateTotals();
            };

            // 6) Первый расчёт
            UpdateTotals();
        }

        #region Filter for Denominations

        private void DenomsFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is Denomination d)
            {
                var flt = DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        #endregion

        #region Touch Mode & Numpad Open

        private static readonly Regex DigitsOnlyRegex = new("[^0-9]+");

        private void DataGrid_PreviewTextInput(object _, TextCompositionEventArgs e)
            => e.Handled = DigitsOnlyRegex.IsMatch(e.Text);

        private void DenomsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell) &&
                cell.Column.Header?.ToString() == "Кол-во")
            {
                e.Handled    = true;
                _activeCell  = cell;
                _buffer      = ((Denomination)cell.DataContext).Amount.ToString();
                NumpadView.Show(_buffer == "0" ? "" : _buffer);
            }
        }

        private void SummaryGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell)
                && cell.Column.DisplayIndex == 2
                && cell.DataContext is SummaryItem)
            {
                e.Handled    = true;
                _activeCell  = cell;
                var si       = (SummaryItem)cell.DataContext;
                _buffer      = si.Value.TrimEnd(' ', '₽');
                NumpadView.Show(_buffer);
            }
        }

        private void TouchModeButton_Checked(object _, RoutedEventArgs __)   => _isTouchMode = true;
        private void TouchModeButton_Unchecked(object _, RoutedEventArgs __) => _isTouchMode = false;

        #endregion

        #region Cell Editing Commit

        private void DenomsGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Кол-во" &&
                e.EditingElement is TextBox tb &&
                e.Row.Item is Denomination d)
            {
                if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "0";
                d.Amount = int.Parse(tb.Text);
            }
            Dispatcher.InvokeAsync(() =>
            {
                DenomsGrid.Items.Refresh();
                UpdateTotals();
            });
        }

        private void SummaryGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox tb &&
                e.Row.Item is SummaryItem si &&
                si.Description.StartsWith("Expected"))
            {
                if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "0";
                si.Value = tb.Text;
            }
            Dispatcher.InvokeAsync(() =>
            {
                SummaryGrid.Items.Refresh();
                UpdateTotals();
            });
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
                SummaryItems[2].Value  = "—";
                SummaryItems[2].Status = SummaryStatus.None;
            }
            else
            {
                var diff = _calcService.CalculateDifference(total, expected);
                SummaryItems[2].Value  = $"{diff} ₽";
                SummaryItems[2].Status = _calcService.GetStatus(diff);
            }
        }

        #endregion

        #region Copy & Clear

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
            foreach (var d in Denominations) d.Amount = 0;
            DenomsGrid.Items.Refresh();
            SummaryItems[1].Value = string.Empty;
            SummaryGrid.Items.Refresh();
            UpdateTotals();
        }

        #endregion

        #region Helpers

        private static bool TryFindParent<T>(DependencyObject? start, out T parent)
            where T : DependencyObject
        {
            parent = null!;
            while (start != null)
            {
                if (start is T found) { parent = found; return true; }
                start = VisualTreeHelper.GetParent(start);
            }
            return false;
        }

        #endregion

        #region Save Settings

        public AppSettings GetCurrentSettings()
        {
            var cfg = new AppSettings
            {
                Filters       = DenominationFilters
                                  .Select(f => new DenominationFilterState { Value = f.Value, IsVisible = f.IsVisible })
                                  .ToList(),
                Denominations = Denominations
                                  .Select(d => new DenominationState { Value = d.Value, Amount = d.Amount })
                                  .ToList(),
                LastExpected  = int.TryParse(SummaryItems[1].Value, out var exp) && exp >= 0 ? exp : 0
            };
            var tot = _calcService.CalculateTotal(Denominations);
            cfg.LastTotal      = tot;
            cfg.LastDifference = tot - cfg.LastExpected;
            return cfg;
        }

        #endregion
    }
}