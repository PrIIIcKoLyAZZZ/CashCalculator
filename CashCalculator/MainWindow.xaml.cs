using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CashCalculator.Models;

namespace CashCalculator
{
    /// <summary>
    /// Interaction logic for <see cref="MainWindow"/>.
    /// Hosts the denominations table, summary table, and touch-friendly numpad popup.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CashRegister _register = new();

        private bool _isTouchMode;
        private string _buffer = string.Empty;
        private DataGridCell? _activeCell;

        /// <summary>
        /// Gets the collection of available denominations bound to the main DataGrid.
        /// </summary>
        public ObservableCollection<Denomination> Denominations { get; }

        /// <summary>
        /// Gets the collection of summary items ("Total", "Expected", "Difference") bound to the summary DataGrid.
        /// </summary>
        public ObservableCollection<SummaryItem> SummaryItems { get; }

        /// <summary>
        /// Gets the collection of visibility filters for each denomination, bound to the settings UI.
        /// </summary>
        public ObservableCollection<DenominationFilter> DenominationFilters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class,
        /// sets up data collections, bindings, and loads initial totals.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Denominations = new(_register.GetDenominations());

            SummaryItems = new()
            {
                new("Сумма",             "0 ₽", SummaryStatus.None),
                new("Должно получиться", "",    SummaryStatus.None),
                new("Расхождение",       "—",   SummaryStatus.None)
            };

            DenominationFilters = new(
                Denominations.Select(d =>
                {
                    var f = new DenominationFilter(d.Value, true);
                    f.PropertyChanged += (_, __) =>
                        ((CollectionViewSource)FindResource("DenomsViewSource")).View.Refresh();
                    return f;
                }));

            DataContext = this;
            ((CollectionViewSource)FindResource("DenomsViewSource")).Source = Denominations;
            SummaryGrid.ItemsSource = SummaryItems;

            UpdateTotals();
        }

        #region Touch Mode

        /// <summary>
        /// Enables touch (numpad) input mode when the toggle button is checked.
        /// </summary>
        private void TouchModeButton_Checked(object _, RoutedEventArgs __) => _isTouchMode = true;

        /// <summary>
        /// Disables touch input mode when the toggle button is unchecked.
        /// </summary>
        private void TouchModeButton_Unchecked(object _, RoutedEventArgs __) => _isTouchMode = false;

        /// <summary>
        /// Opens the numpad popup with the specified initial buffer value.
        /// </summary>
        /// <param name="cell">The cell that will receive the numpad input.</param>
        /// <param name="initial">The initial numeric string to populate in the buffer.</param>
        private void OpenNumpad(DataGridCell cell, string initial)
        {
            _activeCell = cell;
            _buffer = initial;
            NumpadDisplay.Text = initial;
            NumpadPopup.IsOpen = true;
        }

        /// <summary>
        /// Handles mouse clicks on the denominations grid to show numpad in touch mode.
        /// </summary>
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

        /// <summary>
        /// Handles mouse clicks on the summary grid to show numpad for the "Expected" field in touch mode.
        /// </summary>
        private void SummaryGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;

            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell) &&
                cell.Column.DisplayIndex == 2 &&
                cell.DataContext is SummaryItem si &&
                si.Description.StartsWith("Должно"))
            {
                e.Handled = true;
                var val = si.Value.TrimEnd(' ', '₽');
                OpenNumpad(cell, val);
            }
        }

        #endregion

        #region Numpad Input

        /// <summary>
        /// Appends a digit to the numpad buffer and updates the display.
        /// </summary>
        private void Numpad_OnDigit(object sender, RoutedEventArgs _) 
        {
            _buffer += ((Button)sender).Content;
            NumpadDisplay.Text = _buffer;
        }

        /// <summary>
        /// Removes the last character from the numpad buffer.
        /// </summary>
        private void Numpad_OnBackspace(object _, RoutedEventArgs __)
        {
            if (_buffer.Length > 0)
                _buffer = _buffer[..^1];
            NumpadDisplay.Text = _buffer;
        }

        /// <summary>
        /// Commits the buffer to the active cell and closes the numpad popup.
        /// </summary>
        private void Numpad_OnEnter(object _, RoutedEventArgs __)
        {
            CommitBuffer();
            NumpadPopup.IsOpen = false;
        }

        /// <summary>
        /// Writes the buffered value into the appropriate data context (denomination or expected),
        /// refreshes the grids, updates totals, and resets the buffer.
        /// </summary>
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
                si.Value = string.IsNullOrEmpty(_buffer) ? string.Empty : _buffer;
                SummaryGrid.Items.Refresh();
            }

            UpdateTotals();
            _buffer = string.Empty;
            _activeCell = null;
        }

        #endregion

        #region Keyboard Input Validation

        private static readonly Regex DigitsOnly = new("[^0-9]+");

        /// <summary>
        /// Prevents non-digit characters from being entered in the DataGrids.
        /// </summary>
        private void DataGrid_PreviewTextInput(object _, TextCompositionEventArgs e) 
            => e.Handled = DigitsOnly.IsMatch(e.Text);

        #endregion

        #region Cell Edit Handling

        /// <summary>
        /// Commits manual text edits in the denominations grid and updates totals.
        /// </summary>
        private void DenomsGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Кол-во" &&
                e.EditingElement is TextBox tb &&
                e.Row.Item is Denomination d)
            {
                d.Amount = string.IsNullOrWhiteSpace(tb.Text) ? 0 : int.Parse(tb.Text);
            }
            Dispatcher.InvokeAsync(UpdateTotals);
        }

        /// <summary>
        /// Triggers totals recalculation when editing ends in the summary grid.
        /// </summary>
        private void SummaryGrid_CellEditEnding(object _, DataGridCellEditEndingEventArgs __) 
            => Dispatcher.InvokeAsync(UpdateTotals);

        #endregion

        #region Filtering

        /// <summary>
        /// Filters the denominations view based on the user's visibility settings.
        /// </summary>
        private void DenomsFilter(object _, FilterEventArgs e)
        {
            if (e.Item is Denomination d)
            {
                var flt = DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        #endregion

        #region Totals Calculation

        /// <summary>
        /// Updates the summary items: total sum, expected value, and difference with status.
        /// </summary>
        private void UpdateTotals()
        {
            SummaryItems[0].Value = $"{_register.TotalSum()} ₽";

            bool valid = int.TryParse(SummaryItems[1].Value, out int expected) && expected >= 0;
            int delta = valid ? _register.CalculateDifference(expected) : 0;

            if (!valid)
            {
                SummaryItems[2].Value = "—";
                SummaryItems[2].Status = SummaryStatus.None;
            }
            else
            {
                SummaryItems[2].Value = $"{delta} ₽";
                SummaryItems[2].Status = delta switch
                {
                    0   => SummaryStatus.OK,
                    < 0 => SummaryStatus.Under,
                    > 0 => SummaryStatus.Over
                };
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Copies the textual cash count report to the clipboard.
        /// </summary>
        private void CopyReport_Click(object _, RoutedEventArgs __)
        {
            UpdateTotals();
            Clipboard.SetText(_register.ToString());
        }

        /// <summary>
        /// Clears all denomination quantities and the expected value,
        /// then updates totals.
        /// </summary>
        private void Clean_Click(object _, RoutedEventArgs __)
        {
            foreach (var d in Denominations)
                d.Amount = 0;
            SummaryItems[1].Value = string.Empty;
            UpdateTotals();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Attempts to find an ancestor of the specified type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of ancestor to find.</typeparam>
        /// <param name="start">The starting element for the search.</param>
        /// <param name="parent">When this method returns, contains the found ancestor, if any.</param>
        /// <returns>True if an ancestor of type <typeparamref name="T"/> was found; otherwise, false.</returns>
        private static bool TryFindParent<T>(DependencyObject start, out T parent) where T : DependencyObject
        {
            DependencyObject? cur = start;
            while (cur != null)
            {
                if (cur is T p)
                {
                    parent = p;
                    return true;
                }
                cur = VisualTreeHelper.GetParent(cur);
            }
            parent = null!;
            return false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Exposed for <c>App.xaml.cs</c> to trigger a totals recalculation after loading settings.
        /// </summary>
        public void RecalculateTotals() => UpdateTotals();

        #endregion
    }
}