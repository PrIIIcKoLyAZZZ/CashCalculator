using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CashCalculator.Models;

namespace CashCalculator.Views
{
    public partial class DenominationsView : UserControl
    {
        private readonly CollectionViewSource _viewSource;

        public DenominationsView()
        {
            InitializeComponent();
            _viewSource = new CollectionViewSource();
            _viewSource.Filter += ViewSource_Filter;
            DenomsGrid.ItemsSource = _viewSource.View;
        }

        /// <summary>Событие: клик по ячейке "Кол-во". Аргумент — Value номинала.</summary>
        public event EventHandler<int>? QuantityCellClicked;

        /// <summary>Обновляет содержимое списка и фильтров.</summary>
        public void Refresh(
            ObservableCollection<Denomination> denominations,
            ObservableCollection<DenominationFilter> filters)
        {
            // Отпишемся от старых PropertyChanged
            if (_viewSource.Source is ObservableCollection<Denomination> oldDenoms)
                foreach (var d in oldDenoms)
                    d.PropertyChanged -= OnDenominationChanged;

            // Подпишемся на новые
            foreach (var d in denominations)
                d.PropertyChanged += OnDenominationChanged;

            // Подписка на изменение коллекций, чтобы обновлять фильтр
            denominations.CollectionChanged += (_, __) => _viewSource.View.Refresh();
            filters.      CollectionChanged += (_, __) => _viewSource.View.Refresh();
            foreach (var f in filters)
                f.PropertyChanged += (_, __) => _viewSource.View.Refresh();

            _viewSource.Source = denominations;
            _viewSource.View.Refresh();
        }

        private void OnDenominationChanged(object sender, PropertyChangedEventArgs e)
        {
            // ничего тут не делаем, пересчёт в MainWindow
        }

        private void ViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is Denomination d))
            {
                e.Accepted = true;
                return;
            }

            // смотрим на фильтры из DataContext (MainWindow)
            if (DataContext is MainWindow mw)
            {
                var flt = mw.DenominationFilters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void DenomsGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TryFindParent<DataGridCell>(e.OriginalSource as DependencyObject, out var cell)
                && cell.Column.Header?.ToString() == "Кол-во"
                && cell.DataContext is Denomination d)
            {
                QuantityCellClicked?.Invoke(this, d.Value);
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