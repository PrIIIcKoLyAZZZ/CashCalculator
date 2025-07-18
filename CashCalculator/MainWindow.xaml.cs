using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;                        // для FirstOrDefault()
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CashCalculator.Models;

namespace CashCalculator
{
    public partial class MainWindow : Window
    {
        private readonly CashRegister _register = new CashRegister();

        public ObservableCollection<Denomination> Denominations { get; }
        public ObservableCollection<SummaryItem> SummaryItems { get; }
        public ObservableCollection<DenominationFilter> DenominationFilters { get; }

        public MainWindow()
        {
            InitializeComponent();

            // 1) Номиналы
            Denominations = new ObservableCollection<Denomination>(_register.GetDenominations());

            // 2) Итоги
            SummaryItems = new ObservableCollection<SummaryItem>
            {
                new SummaryItem("Сумма",              "0 ₽", SummaryStatus.None),
                new SummaryItem("Должно получиться",  "",   SummaryStatus.None),
                new SummaryItem("Расхождение",        "—",  SummaryStatus.None),
            };

            // 3) Готовим фильтры
            DenominationFilters = new ObservableCollection<DenominationFilter>(
                Denominations.Select(d => {
                  var f = new DenominationFilter(d.Value, true);
                  f.PropertyChanged += (_,__)=>
                      ((CollectionViewSource)FindResource("DenomsViewSource")).View.Refresh();
                  return f;
                })
            );

            // 4) Устанавливаем DataContext и источники
            DataContext = this;
            ((CollectionViewSource)FindResource("DenomsViewSource")).Source = Denominations;

            DenomsGrid.ItemsSource   = ((CollectionViewSource)FindResource("DenomsViewSource")).View;
            SummaryGrid.ItemsSource  = SummaryItems;

            // 5) Скругление углов
            DenomBorder.SizeChanged   += ClipBorder(DenomBorder);
            SummaryBorder.SizeChanged += ClipBorder(SummaryBorder);

            // 6) Первый расчёт
            UpdateTotals();
        }

        private void DenomsFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is Denomination d)
            {
                var flt = DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        private SizeChangedEventHandler ClipBorder(Border b) => (s,e)=>
        {
            b.Clip = new RectangleGeometry(
              new Rect(0,0,b.ActualWidth,b.ActualHeight),
              b.CornerRadius.TopLeft, b.CornerRadius.TopLeft);
        };

        // Только цифры
        private static readonly Regex _digitsOnly = new Regex("[^0-9]+");
        private void DataGrid_PreviewTextInput(object s, TextCompositionEventArgs e)
            => e.Handled = _digitsOnly.IsMatch(e.Text);

        private void DenomsGrid_CellEditEnding(object s, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);

        private void SummaryGrid_CellEditEnding(object s, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);


        private void UpdateTotals()
        {
            // сумма
            int sum = _register.TotalSum();
            SummaryItems[0].Value = $"{sum} ₽";

            // ожидаемая
            var exp = SummaryItems[1];
            bool ok = int.TryParse(exp.Value, out int expected) && expected >= 0;
            if (!ok) expected = -1;

            // расхождение
            var diff = SummaryItems[2];
            if (expected < 0)
            {
                diff.Value  = "—";
                diff.Status = SummaryStatus.None;
            }
            else
            {
                int d = _register.CalculateDifference(expected);
                diff.Value  = $"{d} ₽";
                diff.Status = d switch {
                  0   => SummaryStatus.OK,
                  < 0 => SummaryStatus.Under,
                  > 0 => SummaryStatus.Over
                };
            }

            DenomsGrid.Items.Refresh();
            SummaryGrid.Items.Refresh();
        }

        private void CopyReport_Click(object s, RoutedEventArgs e)
        {
            UpdateTotals();
            Clipboard.SetText(_register.ToString());
        }

        private void AutoUpdateCheckBox_Checked(object s, RoutedEventArgs e)   { /*…*/ }
        private void AutoUpdateCheckBox_Unchecked(object s, RoutedEventArgs e) { /*…*/ }
        private void DarkThemeCheckBox_Checked(object s, RoutedEventArgs e)     { /*…*/ }
        private void DarkThemeCheckBox_Unchecked(object s, RoutedEventArgs e)   { /*…*/ }
    }
}