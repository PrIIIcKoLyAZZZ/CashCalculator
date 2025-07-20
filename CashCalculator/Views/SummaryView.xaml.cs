using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CashCalculator.Models;

namespace CashCalculator.Views
{
    /// <summary>
    /// Interaction logic for SummaryView.xaml
    /// </summary>
    public partial class SummaryView : UserControl
    {
        /// <summary>
        /// Пользователь кликнул по ячейке ожидаемого значения.
        /// </summary>
        public event EventHandler? ExpectedCellClicked;

        public SummaryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обновляет таблицу результатом расчёта.
        /// </summary>
        public void Refresh(ObservableCollection<SummaryItem> items)
        {
            SummaryGrid.ItemsSource = items;
        }

        private void SummaryGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TryFindParent<DataGridCell>((DependencyObject)e.OriginalSource, out var cell)
                && cell.Column.DisplayIndex == 2
                && cell.DataContext is SummaryItem si
                && si.Description.StartsWith("Expected"))
            {
                ExpectedCellClicked?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private bool TryFindParent<T>(DependencyObject source, out T parent)
            where T : DependencyObject
        {
            while (source != null)
            {
                if (source is T found)
                {
                    parent = found;
                    return true;
                }
                source = VisualTreeHelper.GetParent(source);
            }
            parent = null!;
            return false;
        }
    }
}