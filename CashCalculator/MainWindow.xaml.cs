using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private readonly CashRegister _register = new();

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
                new("Сумма",              "0 ₽", SummaryStatus.None),
                new("Должно получиться",  "",   SummaryStatus.None),
                new("Расхождение",        "—",  SummaryStatus.None),
            };

            // 3) Фильтры
            DenominationFilters = new ObservableCollection<DenominationFilter>(
                Denominations.Select(d =>
                {
                    var f = new DenominationFilter(d.Value, true);
                    f.PropertyChanged += (_, __) =>
                        ((CollectionViewSource)FindResource("DenomsViewSource")).View.Refresh();
                    return f;
                })
            );

            // 4) DataContext и источники
            DataContext = this;
            var cvs = (CollectionViewSource)FindResource("DenomsViewSource");
            cvs.Source = Denominations;
            DenomsGrid.ItemsSource  = cvs.View;
            SummaryGrid.ItemsSource = SummaryItems;

            // 5) Скруглённые углы
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

        private SizeChangedEventHandler ClipBorder(Border b) => (s, e) =>
        {
            b.Clip = new RectangleGeometry(
                new Rect(0, 0, b.ActualWidth, b.ActualHeight),
                b.CornerRadius.TopLeft,
                b.CornerRadius.TopLeft);
        };

        // Ввод только цифр
        private static readonly Regex _digitsOnly = new("[^0-9]+");
        private void DataGrid_PreviewTextInput(object s, TextCompositionEventArgs e) =>
            e.Handled = _digitsOnly.IsMatch(e.Text);

        private void DenomsGrid_CellEditEnding(object s, DataGridCellEditEndingEventArgs e) =>
            Dispatcher.InvokeAsync(UpdateTotals);

        private void SummaryGrid_CellEditEnding(object s, DataGridCellEditEndingEventArgs e) =>
            Dispatcher.InvokeAsync(UpdateTotals);

        private void UpdateTotals()
        {
            // 1) Сумма
            int sum = _register.TotalSum();
            SummaryItems[0].Value = $"{sum} ₽";

            // 2) Ожидаемая
            var exp = SummaryItems[1];
            bool ok = int.TryParse(exp.Value, out int expected) && expected >= 0;
            if (!ok) expected = -1;

            // 3) Расхождение и статус
            var diff = SummaryItems[2];
            if (expected < 0)
            {
                diff.Value  = "—";
                diff.Status = SummaryStatus.None;
            }
            else
            {
                int delta = _register.CalculateDifference(expected);
                diff.Value  = $"{delta} ₽";
                diff.Status = delta switch
                {
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

        /// <summary>
        /// Чистит таблицы: сбрасывает все количества купюр и очищает поле «Должно получиться»,
        /// но не трогает состояние фильтров.
        /// </summary>
        private void Clean_Click(object s, RoutedEventArgs e)
        {
            foreach (var denom in Denominations)
                denom.Amount = 0;

            SummaryItems[1].Value = string.Empty;
            UpdateTotals();
        }

        private void AutoUpdateCheckBox_Checked(object s, RoutedEventArgs e)   { /*…*/ }
        private void AutoUpdateCheckBox_Unchecked(object s, RoutedEventArgs e) { /*…*/ }
        private void DarkThemeCheckBox_Checked(object s, RoutedEventArgs e)     { /*…*/ }
        private void DarkThemeCheckBox_Unchecked(object s, RoutedEventArgs e)   { /*…*/ }
    }
}