using System.Windows;
using CashCalculator.Services;
using CashCalculator.Services.Calculation;
using CashCalculator.Services.Settings;

namespace CashCalculator
{
    /// <summary>
    /// Interaction logic for <see cref="App"/>.  
    /// Responsible for application startup, shutdown, and service initialization.
    /// </summary>
    public partial class App : Application
    {
        private ISettingsService        _settingsService = null!;
        private ICashCalculationService _calcService     = null!;
        private MainWindow              _mainWindow      = null!;

        /// <summary>
        /// Called when the application starts.  
        /// Initializes services, loads settings, and creates the main window.
        /// </summary>
        /// <param name="e">Event arguments for startup.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Initialize services
            _settingsService = new JsonSettingsService();
            _calcService     = new CashCalculationService();

            // 2. Load persisted settings
            var settings = _settingsService.Load();

            // 3. Create and show MainWindow, injecting services and settings
            _mainWindow = new MainWindow(_calcService, _settingsService, settings);
            _mainWindow.Show();
        }

        /// <summary>
        /// Called when the application is exiting.  
        /// Gathers current UI state and persists settings.
        /// </summary>
        /// <param name="e">Event arguments for exit.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // 4. Collect current settings from MainWindow and save
            var settings = _mainWindow.GetCurrentSettings();
            _settingsService.Save(settings);

            base.OnExit(e);
        }
    }
}