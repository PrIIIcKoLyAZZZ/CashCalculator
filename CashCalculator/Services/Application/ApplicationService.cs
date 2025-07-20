using CashCalculator.Infrastructure.Data;
using CashCalculator.Services.Calculation;
using CashCalculator.Services.Data;
using CashCalculator.Services.Settings;
using CashCalculator.Services.Interfaces;

namespace CashCalculator.Services.Application
{
    /// <summary>
    /// Управляет стартом и остановкой приложения:
    /// - загружает и сохраняет AppSettings
    /// - загружает и сохраняет CalculationData
    /// - создаёт и показывает MainWindow
    /// </summary>
    public class ApplicationService
    {
        private readonly ISettingsService        _settingsService;
        private readonly ICalculationDataService _calcDataService;
        private readonly ICashCalculationService _calcService;
        private MainWindow                       _mainWindow = null!;

        public ApplicationService()
        {
            _settingsService = new JsonSettingsService();
            _calcDataService = new JsonCalculationDataService(); // ваш сервис
            _calcService     = new CashCalculationService();
        }

        /// <summary>
        /// Вызывается из App.OnStartup:
        /// 1) Загружает AppSettings
        /// 2) Загружает CalculationData
        /// 3) Создаёт MainWindow(calcService, settings, calcData) и показывает его
        /// </summary>
        public void Startup()
        {
            var settings = _settingsService.Load();
            var calcData = _calcDataService.Load();

            _mainWindow = new MainWindow(_calcService, settings, calcData);
            _mainWindow.Show();
        }

        /// <summary>
        /// Вызывается из App.OnExit:
        /// 1) Читает актуальные AppSettings и CalculationData из MainWindow
        /// 2) Сохраняет их в свои сервисы
        /// </summary>
        public void Shutdown()
        {
            var settings = _mainWindow.GetAppSettings();
            var calcData = _mainWindow.GetCalculationData();

            _settingsService.Save(settings);
            _calcDataService.Save(calcData);
        }
    }
}