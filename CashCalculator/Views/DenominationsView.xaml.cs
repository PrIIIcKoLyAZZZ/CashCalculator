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
        private ObservableCollection<Denomination>?      _denominations;
        private ObservableCollection<DenominationFilter>? _filters;

        public event EventHandler<int>? QuantityCellClicked;

        public DenominationsView()
        {
            InitializeComponent();
            _viewSource = new CollectionViewSource();
            _viewSource.Filter += ViewSource_Filter;
            DenomsGrid.ItemsSource = _viewSource.View;
        }

        public void Refresh(
            ObservableCollection<Denomination> denominations,
            ObservableCollection<DenominationFilter> filters)
        {
            if (_filters != null)
                foreach (var f in _filters)
                    f.PropertyChanged -= OnFilterChanged;

            _denominations = denominations;
            _filters       = filters;

            foreach (var f in _filters)
                f.PropertyChanged += OnFilterChanged;

            _viewSource.Source = _denominations;
            _viewSource.View.Refresh();
        }

        private void OnFilterChanged(object? s, PropertyChangedEventArgs e) 
            => _viewSource.View.Refresh();

        private void ViewSource_Filter(object s, FilterEventArgs e)
        {
            if (_filters == null) { e.Accepted = true; return; }
            if (e.Item is Denomination d)
            {
                var flt = _filters.FirstOrDefault(f => f.Value == d.Value);
                e.Accepted = flt?.IsVisible ?? true;
            }
        }

        private void DenomsGrid_PreviewMouseLeftButtonDown(object s, System.Windows.Input.MouseButtonEventArgs e)
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
                if (start is T found) { parent = found; return true; }
                start = VisualTreeHelper.GetParent(start);
            }
            return false;
        }
    }
}