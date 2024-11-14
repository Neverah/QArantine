using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;

using QArantine.Code.QArantineGUI.Models;

namespace QArantine.Code.QArantineGUI.Services
{
    public class WindowSettingsService
    {
        private const string SettingsFile = "QArantine/Config/GUIWindowSettings.json";

        public void LoadWindowSize(Window? window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settingsList = JsonSerializer.Deserialize<List<WindowSettings>>(json);
                var settings = settingsList?.FirstOrDefault(s => s.WindowTitle == window.Title);
                if (settings != null)
                {
                    window.Width = settings.Width;
                    window.Height = settings.Height;
                    window.Position = new PixelPoint(settings.PosX, settings.PosY);
                    return;
                }
            }
            // Default case
            window.Width = 800;
            window.Height = 900;
        }

        public void SaveWindowSizeAndPos(Window? window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            List<WindowSettings> settingsList;
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                settingsList = JsonSerializer.Deserialize<List<WindowSettings>>(json) ?? new List<WindowSettings>();
            }
            else
            {
                settingsList = new List<WindowSettings>();
            }

            var existingSettings = settingsList.FirstOrDefault(s => s.WindowTitle == window.Title);
            if (existingSettings != null)
            {
                existingSettings.Width = window.Width;
                existingSettings.Height = window.Height;
                existingSettings.PosX = window.Position.X;
                existingSettings.PosY = window.Position.Y;
            }
            else
            {
                var settings = new WindowSettings(window.Title!, window.Width, window.Height, window.Position.X, window.Position.Y);
                settingsList.Add(settings);
            }

            var updatedJson = JsonSerializer.Serialize(settingsList, new JsonSerializerOptions { WriteIndented = true });

            string? directoryPath = Path.GetDirectoryName(SettingsFile);
            if (directoryPath != null) Directory.CreateDirectory(directoryPath);
            
            File.WriteAllText(SettingsFile, updatedJson);
        }
    }
}
