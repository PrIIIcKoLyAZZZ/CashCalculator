using System;
using System.IO;
using System.Text.Json;

namespace CashCalculator.Infrastructure
{
    /// <summary>
    /// A generic repository that loads and saves instances of <typeparamref name="T"/>
    /// as human-readable JSON files in the user's Application Data folder.
    /// Files are stored under:
    /// %APPDATA%\KremenchugskayaTeam\CashCalculator\&lt;fileName&gt;.
    /// </summary>
    /// <typeparam name="T">The type of object to persist. Must have a parameterless constructor.</typeparam>
    public class JsonRepository<T> where T : class, new()
    {
        /// <summary>
        /// The full path to the JSON file.
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// JSON serialization options: indented, human-readable.
        /// </summary>
        private readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRepository{T}"/> class.
        /// Ensures that the storage directory exists.
        /// </summary>
        /// <param name="fileName">
        /// The name of the JSON file (including extension) to load and save.
        /// For example: "appsettings.json".
        /// </param>
        public JsonRepository(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder  = Path.Combine(appData, "KremenchugskayaTeam", "CashCalculator");
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, fileName);
        }

        /// <summary>
        /// Loads the JSON file and deserializes its content to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>
        /// An instance of <typeparamref name="T"/> populated from the JSON file,
        /// or a new default instance if the file does not exist or deserialization fails.
        /// </returns>
        public T Load()
        {
            if (!File.Exists(_filePath))
                return new T(); // File has not been created yet

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<T>(json, _opts) ?? new T();
        }

        /// <summary>
        /// Serializes the provided data to JSON and writes it to the file.
        /// </summary>
        /// <param name="data">The object to serialize and save.</param>
        public void Save(T data)
        {
            string json = JsonSerializer.Serialize(data, _opts);
            File.WriteAllText(_filePath, json);
        }
    }
}