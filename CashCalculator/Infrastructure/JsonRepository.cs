using System;
using System.IO;
using System.Text.Json;

namespace CashCalculator.Infrastructure
{
    public class JsonRepository<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { WriteIndented = true };

        public JsonRepository(string fileName)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder  = Path.Combine(appData, "KremenchugskayaTeam", "CashCalculator");
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, fileName);
        }

        public T Load()
        {
            if (!File.Exists(_filePath))
                return new T();      // возвращаем пустой, если файла нет

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<T>(json, _opts) ?? new T();
        }

        public void Save(T data)
        {
            var json = JsonSerializer.Serialize(data, _opts);
            File.WriteAllText(_filePath, json);
        }
    }
}