using System.Windows;
using CashCalculator.Services.Application;
using CashCalculator.Services.Calculation;
using CashCalculator.Services.Data;
using CashCalculator.Services.Settings;

namespace CashCalculator
{
    public partial class App : Application
    {
        private ApplicationService _applicationService = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settingsService = new JsonSettingsService();
            var calcDataService = new JsonCalculationDataService();
            var calcService     = new CashCalculationService();

            _applicationService = new ApplicationService(
                settingsService,
                calcDataService,
                calcService);

            _applicationService.Startup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationService.Shutdown();
            base.OnExit(e);
        }
    }
}