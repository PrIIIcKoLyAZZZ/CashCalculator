using CashCalculator.Infrastructure;

namespace CashCalculator.Services
{
    /// <summary>
    /// Defines methods for loading and saving application settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Loads persisted application settings.
        /// </summary>
        /// <returns>An <see cref="AppSettings"/> instance containing stored settings.</returns>
        AppSettings Load();

        /// <summary>
        /// Saves the provided application settings.
        /// </summary>
        /// <param name="settings">The <see cref="AppSettings"/> to persist.</param>
        void Save(AppSettings settings);
    }
}