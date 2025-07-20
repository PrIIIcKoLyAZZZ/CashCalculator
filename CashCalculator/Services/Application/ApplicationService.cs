using CashCalculator.Infrastructure;
using CashCalculator.Services.Calculation;
using CashCalculator.Services.Settings;
using CashCalculator.Services.Interfaces;

namespace CashCalculator.Services.Application
{
    public class ApplicationService
    {
        private readonly ISettingsService        _settingsService;
        private readonly ICashCalculationService _calcService;
        private MainWindow                       _mainWindow = null!;

        public ApplicationService()
        {
            _settingsService = new JsonSettingsService();
            _calcService     = new CashCalculationService();
        }

        public void Startup()
        {
            var settings = _settingsService.Load();
            _mainWindow  = new MainWindow(_calcService, settings);
            _mainWindow.Show();
        }

        public void Shutdown()
        {
            var settings = _mainWindow.GetCurrentSettings();
            _settingsService.Save(settings);
        }
    }
}