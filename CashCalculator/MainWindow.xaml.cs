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
using System.Windows.Media.Animation;
using CashCalculator.Models;

namespace CashCalculator
{
    public partial class MainWindow : Window
    {
        private readonly CashRegister _register = new();
        private bool   _isTouchMode;
        private string _buffer = "";
        private DataGridCell _activeCell;

        public ObservableCollection<Denomination>       Denominations       { get; }
        public ObservableCollection<SummaryItem>        SummaryItems        { get; }
        public ObservableCollection<DenominationFilter> DenominationFilters { get; }

        public MainWindow()
        {
            InitializeComponent();

            /* ---- модели ---- */
            Denominations = new(_register.GetDenominations());
            SummaryItems  = new()
            {
                new("Сумма",             "0 ₽", SummaryStatus.None),
                new("Должно получиться", "",    SummaryStatus.None),
                new("Расхождение",       "—",   SummaryStatus.None)
            };
            DenominationFilters = new(
                Denominations.Select(d =>
                {
                    var f = new DenominationFilter(d.Value, true);
                    f.PropertyChanged += (_,__) =>
                        ((CollectionViewSource)FindResource("DenomsViewSource")).View.Refresh();
                    return f;
                }));

            DataContext = this;

            ((CollectionViewSource)FindResource("DenomsViewSource")).Source = Denominations;
            SummaryGrid.ItemsSource = SummaryItems;            // ← таблица «Итого»

            /* ---- анимация Popup ---- */
            NumpadPopup.Opened += (_,__) =>
            {
                if (NumpadPopup.Child is Border b &&
                    b.RenderTransform is ScaleTransform st)
                {
                    var anim = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(150))
                    { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
            };

            UpdateTotals();
        }

        /* ---------- Touch-режим ---------- */
        private void TouchModeButton_Checked  (object s, RoutedEventArgs e) => _isTouchMode = true;
        private void TouchModeButton_Unchecked(object s, RoutedEventArgs e) => _isTouchMode = false;

        private void DenomsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isTouchMode) return;
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell) &&
                cell.Column.Header?.ToString() == "Кол-во")
            {
                e.Handled     = true;
                _activeCell   = cell;
                _buffer       = "";
                NumpadDisplay.Text = "";
                NumpadPopup.IsOpen = true;
            }
        }

        /* ---------- Numpad ---------- */
        private void Numpad_OnDigit(object s, RoutedEventArgs e)
        {
            _buffer += ((Button)s).Content;
            NumpadDisplay.Text = _buffer;
        }

        private void Numpad_OnBackspace(object s, RoutedEventArgs e)
        {
            if (_buffer.Length > 0)
                _buffer = _buffer[..^1];
            NumpadDisplay.Text = _buffer;
        }

        private void Numpad_OnEnter(object s, RoutedEventArgs e)
        {
            if (int.TryParse(_buffer, out int v) && _activeCell?.DataContext is Denomination d)
            {
                d.Amount = v;
                UpdateTotals();
            }
            NumpadPopup.IsOpen = false;
        }

        /* ---------- фильтр и валидация ---------- */
        private static readonly Regex _digitsOnly = new("[^0-9]+");
        private void DataGrid_PreviewTextInput(object s, TextCompositionEventArgs e)
            => e.Handled = _digitsOnly.IsMatch(e.Text);

        private void DenomsGrid_CellEditEnding  (object s, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);
        private void SummaryGrid_CellEditEnding(object s, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);

        private void DenomsFilter(object s, FilterEventArgs e)
        {
            if (e.Item is Denomination d)
            {
                var flt = DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        /* ---------- Totals ---------- */
        private void UpdateTotals()
        {
            SummaryItems[0].Value = $"{_register.TotalSum()} ₽";

            bool ok = int.TryParse(SummaryItems[1].Value, out int expected) && expected >= 0;
            int delta = ok ? _register.CalculateDifference(expected) : 0;

            if (!ok)
            {
                SummaryItems[2].Value  = "—";
                SummaryItems[2].Status = SummaryStatus.None;
            }
            else
            {
                SummaryItems[2].Value  = $"{delta} ₽";
                SummaryItems[2].Status = delta switch
                {
                    0   => SummaryStatus.OK,
                    < 0 => SummaryStatus.Under,
                    > 0 => SummaryStatus.Over
                };
            }

            /* --- ОБНОВЛЯЕМ ТАБЛИЦЫ --- */
            DenomsGrid.Items.Refresh();
            SummaryGrid.Items.Refresh();
        }

        /* ---------- Copy & Clean ---------- */
        private void CopyReport_Click(object s, RoutedEventArgs e)
        {
            UpdateTotals();
            Clipboard.SetText(_register.ToString());
        }

        private void Clean_Click(object s, RoutedEventArgs e)
        {
            foreach (var d in Denominations) d.Amount = 0;
            SummaryItems[1].Value = "";
            UpdateTotals();          // вызовет Refresh() через UpdateTotals
        }

        /* ---------- helper ---------- */
        private static bool TryFindParent<T>(DependencyObject start, out T parent) where T : DependencyObject
        {
            var cur = start;
            while (cur != null)
            {
                if (cur is T p) { parent = p; return true; }
                cur = VisualTreeHelper.GetParent(cur);
            }
            parent = null;
            return false;
        }
    }
}