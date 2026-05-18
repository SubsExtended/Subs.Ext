// Subs.Ext\Tools\Rating.WPF\Services\SettingsService.cs

using Rating.WPF.General;
using Rating.WPF.Models;
using Rating.WPF.Services;
using System;
using System.IO;
using System.Text.Json;

using static Rating.WPF.General.Constants;

public class SettingsService : ISettingsService
{
    private readonly string _folder;
    private readonly string _filePath;

    public AppSettings Settings { get; private set; }

    public SettingsService()
    {
        _folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.AppShortName);

        _filePath = Path.Combine(_folder, "settings.json");

        Settings = Load();
    }

    public AppSettings Load()
    {
        try
        {
            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);

            if (!File.Exists(_filePath))
                return new AppSettings();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);

            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Optional: show a dialog or log
        }
    }
}