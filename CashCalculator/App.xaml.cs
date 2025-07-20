using System.Windows;
using CashCalculator.Services.Application;

namespace CashCalculator
{
    public partial class App : Application
    {
        private readonly ApplicationService _appService = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _appService.Startup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _appService.Shutdown();
            base.OnExit(e);
        }
    }
}