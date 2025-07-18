using System;
using System.Linq;
using System.Windows;
using CashCalculator.Infrastructure;
using CashCalculator.Models;

namespace CashCalculator
{
    /// <summary>
    /// Application entry point for the CashCalculator WPF application.
    /// Handles loading and saving of user settings on startup and exit.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Repository for persisting <see cref="AppSettings"/> to a JSON file.
        /// </summary>
        private readonly JsonRepository<AppSettings> _repo =
            new JsonRepository<AppSettings>("settings.json");

        /// <summary>
        /// The settings loaded from the JSON file at application startup.
        /// </summary>
        public static AppSettings CurrentSettings { get; private set; } = null!;

        /// <summary>
        /// Reference to the main window, needed for applying and saving settings.
        /// </summary>
        private MainWindow _mainWindow = null!;

        /// <summary>
        /// Handles the Startup event of the application.
        /// Loads saved settings, applies them to the main window, and shows the window.
        /// </summary>
        /// <param name="sender">The source of the Startup event.</param>
        /// <param name="e">Event data for the Startup event.</param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 1. Load settings from %APPDATA%\KremenchugskayaTeam\CashCalculator\settings.json
            CurrentSettings = _repo.Load();

            // 2. Create and initialize the main window
            _mainWindow = new MainWindow();

            // 3. Apply saved visibility filters for each denomination
            foreach (var f in _mainWindow.DenominationFilters)
            {
                var savedFilter = CurrentSettings.Filters
                    .FirstOrDefault(x => x.Value == f.Value);
                if (savedFilter != null)
                    f.IsVisible = savedFilter.IsVisible;
            }

            // 4. Apply saved counts for each denomination
            foreach (var d in _mainWindow.Denominations)
            {
                var savedState = CurrentSettings.Denominations
                    .FirstOrDefault(x => x.Value == d.Value);
                if (savedState != null)
                    d.Amount = savedState.Amount;
            }

            // 5. Restore the "expected" target sum
            _mainWindow.SummaryItems[1].Value = CurrentSettings.LastExpected.ToString();

            // 6. Recalculate totals so sum and difference display immediately
            _mainWindow.RecalculateTotals();

            // 7. Show the main window
            _mainWindow.Show();
        }

        /// <summary>
        /// Handles the Exit event of the application.
        /// Gathers current values from the UI and saves them back to the JSON settings file.
        /// </summary>
        /// <param name="sender">The source of the Exit event.</param>
        /// <param name="e">Event data for the Exit event.</param>
        private void OnExit(object sender, ExitEventArgs e)
        {
            // Parse the current expected value, defaulting to 0 if invalid
            int expected = int.TryParse(_mainWindow.SummaryItems[1].Value, out var v) && v >= 0
                ? v
                : 0;

            // Compute the current total sum and difference
            int total = _mainWindow.Denominations.Sum(d => d.Amount * d.Value);
            int diff  = total - expected;

            // Populate settings model for serialization
            var settings = new AppSettings
            {
                Filters = _mainWindow.DenominationFilters
                    .Select(f => new DenominationFilterState
                    {
                        Value     = f.Value,
                        IsVisible = f.IsVisible
                    })
                    .ToList(),

                Denominations = _mainWindow.Denominations
                    .Select(d => new DenominationState
                    {
                        Value  = d.Value,
                        Amount = d.Amount
                    })
                    .ToList(),

                LastExpected   = expected,
                LastTotal      = total,
                LastDifference = diff
            };

            // Save settings back to JSON file
            _repo.Save(settings);
        }
    }
}