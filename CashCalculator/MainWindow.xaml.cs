using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();

            DenomBorder.SizeChanged   += ClipBorder(DenomBorder);
            SummaryBorder.SizeChanged += ClipBorder(SummaryBorder);

            // 1) Номиналы
            Denominations = new ObservableCollection<Denomination>(_register.GetDenominations());
            DenomsGrid.ItemsSource = Denominations;

            // 2) Итоги
            SummaryItems = new ObservableCollection<SummaryItem>
            {
                new SummaryItem("Сумма",               "0 ₽", SummaryStatus.None),
                new SummaryItem("Должно получиться","",    SummaryStatus.None),
                new SummaryItem("Расхождение",         "—",   SummaryStatus.None),
            };
            SummaryGrid.ItemsSource = SummaryItems;

            UpdateTotals();
        }

        private SizeChangedEventHandler ClipBorder(Border b) => (s, e) =>
        {
            b.Clip = new RectangleGeometry(
                new Rect(0, 0, b.ActualWidth, b.ActualHeight),
                b.CornerRadius.TopLeft,
                b.CornerRadius.TopLeft);
        };

        private static readonly Regex _digitsOnly = new Regex("[^0-9]+");
        private void DataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = _digitsOnly.IsMatch(e.Text);

        private void DenomsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);

        private void SummaryGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
            => Dispatcher.InvokeAsync(UpdateTotals);

        private void UpdateTotals()
        {
            int sum = _register.TotalSum();
            SummaryItems[0].Value = $"{sum} ₽";

            var exp = SummaryItems[1];
            bool ok = int.TryParse(exp.Value, out int expected) && expected >= 0;
            if (!ok) expected = -1;

            var diff = SummaryItems[2];
            if (expected < 0)
            {
                diff.Value = "—";
                diff.Status = SummaryStatus.None;
            }
            else
            {
                int delta = _register.CalculateDifference(expected);
                diff.Value = $"{delta} ₽";
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

        private void CopyReport_Click(object sender, RoutedEventArgs e)
        {
            UpdateTotals();
            Clipboard.SetText(_register.ToString());
        }
    }
}