using CashCalculator.Infrastructure;

namespace CashCalculator.Services.Settings
{
    /// <summary>
    /// Provides loading and saving of application settings using a JSON repository.
    /// </summary>
    public class JsonSettingsService : ISettingsService
    {
        private readonly JsonRepository<AppSettings> _repo;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonSettingsService"/>,
        /// using "settings.json" as the storage file.
        /// </summary>
        public JsonSettingsService()
        {
            _repo = new JsonRepository<AppSettings>("settings.json");
        }

        /// <summary>
        /// Loads the application settings from the JSON file.
        /// </summary>
        /// <returns>The loaded <see cref="AppSettings"/> instance.</returns>
        public AppSettings Load() => _repo.Load();

        /// <summary>
        /// Saves the provided application settings to the JSON file.
        /// </summary>
        /// <param name="settings">The <see cref="AppSettings"/> to save.</param>
        public void Save(AppSettings settings) => _repo.Save(settings);
    }
}