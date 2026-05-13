using System.IO;
using System.Text.Json;
using Rating.WPF.Models;

namespace Rating.WPF.Services
{
    public class SettingsService : ISettingsService
    {
        private const string FileName = "settings.json";

        public AppSettings Settings { get; private set; }

        public SettingsService()
        {
            Settings = Load();
        }

        public AppSettings Load()
        {
            if (!File.Exists(FileName))
                return new AppSettings();

            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}