using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CashCalculator.Models;

namespace CashCalculator.Views
{
    public partial class SummaryView : UserControl
    {
        public SummaryView()
        {
            InitializeComponent();
        }

        /// <summary>Событие: клик по ячейке Expected Amount.</summary>
        public event EventHandler? ExpectedCellClicked;

        /// <summary>Обновляет строки итогов.</summary>
        public void Refresh(ObservableCollection<SummaryItem> items)
        {
            SummaryGrid.ItemsSource = items;
        }

        private void SummaryGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TryFindParent<DataGridCell>(e.OriginalSource as DependencyObject, out var cell)
                && cell.Column.DisplayIndex == 2
                && cell.DataContext is SummaryItem si
                && si.Description.StartsWith("Expected"))
            {
                ExpectedCellClicked?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private static bool TryFindParent<T>(DependencyObject? start, out T parent)
            where T : DependencyObject
        {
            parent = null!;
            while (start != null)
            {
                if (start is T found)
                {
                    parent = found;
                    return true;
                }
                start = VisualTreeHelper.GetParent(start);
            }
            return false;
        }
    }
}