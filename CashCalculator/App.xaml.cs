using System;
using System.Linq;
using System.Windows;
using CashCalculator.Infrastructure;
using CashCalculator.Models;

namespace CashCalculator
{
    public partial class App : Application
    {
        // Репозиторий для сериализации настроек
        private readonly JsonRepository<AppSettings> _repo
            = new JsonRepository<AppSettings>("settings.json");

        // Сохранённые настройки приложения
        public static AppSettings CurrentSettings { get; private set; } = null!;

        // Храним ссылку на главное окно, чтобы потом собрать из него настройки
        private MainWindow _mainWindow = null!;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 1. Загружаем настройки из %APPDATA%\…\settings.json
            CurrentSettings = _repo.Load();

            // 2. Создаём окно и сохраняем в поле
            _mainWindow = new MainWindow();

            // 3. Применяем видимость номиналов
            foreach (var filter in _mainWindow.DenominationFilters)
            {
                var saved = CurrentSettings.Filters
                    .FirstOrDefault(x => x.Value == filter.Value);
                if (saved != null)
                    filter.IsVisible = saved.IsVisible;
            }

            // 4. Применяем сохранённые количества купюр
            foreach (var denom in _mainWindow.Denominations)
            {
                var saved = CurrentSettings.Denominations
                    .FirstOrDefault(x => x.Value == denom.Value);
                if (saved != null)
                    denom.Amount = saved.Amount;
            }

            // 5. Применяем последний «должно получиться»
            _mainWindow.SummaryItems[1].Value = 
                CurrentSettings.LastExpected.ToString();

            // 6. Показываем окно
            _mainWindow.Show();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            // Собираем новую модель для сохранения
            var settings = new AppSettings
            {
                Filters = _mainWindow.DenominationFilters
                    .Select(f => new DenominationFilterState {
                        Value     = f.Value,
                        IsVisible = f.IsVisible
                    }).ToList(),

                Denominations = _mainWindow.Denominations
                    .Select(d => new DenominationState {
                        Value  = d.Value,
                        Amount = d.Amount
                    }).ToList(),

                LastExpected = int.TryParse(
                    _mainWindow.SummaryItems[1].Value, 
                    out var exp) && exp >= 0
                    ? exp
                    : 0
            };

            // Сохраняем в JSON
            _repo.Save(settings);
        }
    }
}