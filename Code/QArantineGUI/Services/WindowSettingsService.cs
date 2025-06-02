using System.Text.Json;
using Avalonia;
using Avalonia.Controls;

using QArantine.Code.JsonContexts;
using QArantine.Code.QArantineGUI.Models;

namespace QArantine.Code.QArantineGUI.Services
{
    public class WindowSettingsService
    {
        private const string SettingsFile = "QArantine/Config/GUIWindowSettings.json";

        public static void LoadWindowSize(Window? window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));

            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settingsList = JsonSerializer.Deserialize(json, WindowSettingsJsonContext.Default.ListWindowSettings);
                var settings = settingsList?.FirstOrDefault(s => s.WindowTitle == window.Title);
                if (settings != null)
                {
                    window.Width = settings.Width;
                    window.Height = settings.Height;
                    window.Position = new PixelPoint(settings.PosX, settings.PosY);
                    return;
                }
            }
        }

        public static void SaveWindowSizeAndPos(Window? window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));

            var screens = window.Screens.All;
            bool isPositionValid = false;
            int finalPosX, finalPosY;

            foreach (var screen in screens)
            {
                var screenBounds = screen.Bounds;
                if (screenBounds.Contains(window.Position))
                {
                    isPositionValid = true;
                    break;
                }
            }
            if (isPositionValid)
            {
                finalPosX = window.Position.X;
                finalPosY = window.Position.Y;
            }
            else
            {
                finalPosX = 0;
                finalPosY = 0;
            }

            List<WindowSettings> settingsList;
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                settingsList = JsonSerializer.Deserialize(json, WindowSettingsJsonContext.Default.ListWindowSettings) ?? [];
            }
            else
            {
                settingsList = [];
            }

            var existingSettings = settingsList.FirstOrDefault(s => s.WindowTitle == window.Title);
            if (existingSettings != null)
            {
                existingSettings.Width = window.Width;
                existingSettings.Height = window.Height;
                existingSettings.PosX = finalPosX;
                existingSettings.PosY = finalPosY;
            }
            else
            {
                var settings = new WindowSettings(window.Title!, window.Width, window.Height, finalPosX, finalPosY);
                settingsList.Add(settings);
            }

            var updatedJson = JsonSerializer.Serialize(settingsList, WindowSettingsJsonContext.Default.ListWindowSettings);

            string? directoryPath = Path.GetDirectoryName(SettingsFile);
            if (directoryPath != null) Directory.CreateDirectory(directoryPath);

            File.WriteAllText(SettingsFile, updatedJson);
        }
    }
}
