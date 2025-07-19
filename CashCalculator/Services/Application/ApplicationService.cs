using CashCalculator.Services.Interfaces;

namespace CashCalculator.Services.Application;

/// <summary>
/// Coordinates application startup and shutdown,
/// delegating settings and calculation data persistence.
/// </summary>
public class ApplicationService
{
    private readonly ISettingsService _settingsService;
    private readonly ICalculationDataService _calcDataService;
    private readonly ICashCalculationService _calcService;
    private MainWindow _mainWindow = null!;

    public ApplicationService(
        ISettingsService settingsService,
        ICalculationDataService calcDataService,
        ICashCalculationService calcService)
    {
        _settingsService = settingsService;
        _calcDataService = calcDataService;
        _calcService     = calcService;
    }

    /// <summary>
    /// Loads settings and last session data, then shows the main window.
    /// </summary>
    public void Startup()
    {
        var settings = _settingsService.Load();
        var calcData = _calcDataService.Load();

        _mainWindow = new MainWindow(_calcService, settings, calcData);
        _mainWindow.Show();
    }

    /// <summary>
    /// Gathers current settings and calculation data, then persists both.
    /// </summary>
    public void Shutdown()
    {
        var settings = _mainWindow.GetAppSettings();
        _settingsService.Save(settings);

        var data = _mainWindow.GetCalculationData();
        _calcDataService.Save(data);
    }
}