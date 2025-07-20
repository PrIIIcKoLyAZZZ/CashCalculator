using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CashCalculator.Infrastructure.Data;
using CashCalculator.Infrastructure.States;
using CashCalculator.Models;
using CashCalculator.Services.Interfaces;
using CashCalculator.Views;

namespace CashCalculator
{
    public partial class MainWindow : Window
    {
        private readonly ICashCalculationService _calcService;
        private bool    _isTouchMode;
        private string  _buffer      = string.Empty;
        private object? _activeItem;

        public ObservableCollection<Denomination>       Denominations       { get; }
        public ObservableCollection<DenominationFilter> DenominationFilters { get; }
        public ObservableCollection<SummaryItem>        SummaryItems        { get; }

        public MainWindow(
            ICashCalculationService calcService,
            AppSettings settings,
            CalculationData calcData)
        {
            InitializeComponent();
            DataContext  = this;
            _calcService = calcService;

            // Prepare summary items
            SummaryItems = new ObservableCollection<SummaryItem>
            {
                new("Total",           $"{calcData.LastTotal} ₽",        SummaryStatus.None),
                new("Expected Amount", calcData.LastExpected.ToString(), SummaryStatus.None),
                new("Difference",      $"{calcData.LastDifference} ₽",    SummaryStatus.None)
            };

            // Prepare denominations
            Denominations = new ObservableCollection<Denomination>(
                new CashRegister().GetDenominations()
            );
            foreach (var d in Denominations)
            {
                var saved = calcData.Denominations.FirstOrDefault(x => x.Value == d.Value);
                if (saved != null) d.Amount = saved.Amount;
            }

            // Prepare filters
            DenominationFilters = new ObservableCollection<DenominationFilter>(
                Denominations.Select(d => new DenominationFilter(d.Value, true))
            );
            if (settings.Filters?.Count == DenominationFilters.Count)
            {
                foreach (var sf in settings.Filters)
                {
                    var f = DenominationFilters.First(x => x.Value == sf.Value);
                    f.IsVisible = sf.IsVisible;
                }
            }

            // Refresh child views
            DenominationsView.Refresh(Denominations, DenominationFilters);
            SummaryView.Refresh(SummaryItems);

            // Subscribe to control events
            DenominationsView.QuantityCellClicked += DenominationsView_QuantityCellClicked;
            SummaryView.ExpectedCellClicked      += SummaryView_ExpectedCellClicked;
            ToolbarView.CopyRequested            += (_, __) => CopyReport();
            ToolbarView.ClearRequested           += (_, __) => ClearAll();
            ToolbarView.TouchModeToggled         += (_, on) => _isTouchMode = on;

            // Numpad events
            NumpadView.DigitPressed     += (_, c) =>
            {
                _buffer += c;
                NumpadView.Show(_buffer);
            };
            NumpadView.BackspacePressed += (_, __) =>
            {
                if (_buffer.Length > 0) _buffer = _buffer[..^1];
                NumpadView.Show(_buffer);
            };
            NumpadView.EnterPressed += (_, __) =>
            {
                if (_activeItem is Denomination d)
                    d.Amount = string.IsNullOrEmpty(_buffer) ? 0 : int.Parse(_buffer);
                else if (_activeItem is SummaryItem si)
                    si.Value = string.IsNullOrEmpty(_buffer) ? "0" : _buffer;

                _buffer     = string.Empty;
                _activeItem = null;
                UpdateTotals();
            };

            UpdateTotals();
        }

        private void DenominationsView_QuantityCellClicked(object _, int value)
        {
            if (!_isTouchMode) return;
            var d = Denominations.First(x => x.Value == value);
            _activeItem = d;
            _buffer     = d.Amount.ToString();
            NumpadView.Show(_buffer == "0" ? string.Empty : _buffer);
        }

        private void SummaryView_ExpectedCellClicked(object _, EventArgs __)
        {
            if (!_isTouchMode) return;
            var si = SummaryItems[1];
            _activeItem = si;
            _buffer     = si.Value.TrimEnd(' ', '₽');
            NumpadView.Show(_buffer);
        }

        private void UpdateTotals()
        {
            var total = _calcService.CalculateTotal(Denominations);
            SummaryItems[0].Value = $"{total} ₽";

            if (!int.TryParse(SummaryItems[1].Value, out var expected) || expected < 0)
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

        private void CopyReport()
        {
            UpdateTotals();
            var reg = new CashRegister();
            foreach (var d in Denominations)
                reg.SetCount(d.Value, d.Amount);
            Clipboard.SetText(reg.ToString());
        }

        private void ClearAll()
        {
            foreach (var d in Denominations)
                d.Amount = 0;
            SummaryItems[1].Value = string.Empty;
            UpdateTotals();
        }

        public AppSettings GetAppSettings() => new AppSettings
        {
            Filters = DenominationFilters
                .Select(f => new DenominationFilterState { Value = f.Value, IsVisible = f.IsVisible })
                .ToList()
        };

        public CalculationData GetCalculationData()
        {
            int expected = int.TryParse(SummaryItems[1].Value, out var e) && e >= 0 ? e : 0;
            var total    = _calcService.CalculateTotal(Denominations);
            return new CalculationData
            {
                LastExpected   = expected,
                LastTotal      = total,
                LastDifference = _calcService.CalculateDifference(total, expected),
                Denominations  = Denominations
                    .Select(d => new DenominationState { Value = d.Value, Amount = d.Amount })
                    .ToList()
            };
        }
    }
}