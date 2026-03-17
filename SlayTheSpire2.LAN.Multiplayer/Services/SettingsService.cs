using System.Text.Json;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Models;

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class SettingsService
    {
        private static readonly Lazy<SettingsService> Lazy = new(() => new SettingsService());

        public static SettingsService Instance => Lazy.Value;

        public readonly SettingsModel SettingsModel;

        private readonly GodotFileIo _modsDir =
            new(Path.Combine(UserDataPathProvider.GetAccountScopedBasePath(null), "mods"));

        private SettingsService()
        {
            if (_modsDir.FileExists("lan_settings.json"))
            {
                SettingsModel =
                    JsonSerializer.Deserialize<SettingsModel>(_modsDir.ReadFile("lan_settings.json") ?? string.Empty) ??
                    new SettingsModel();
            }
            else
            {
                SettingsModel = new SettingsModel();
            }
        }

        public void WriteSettings()
        {
            _modsDir.WriteFile("lan_settings.json",
                JsonSerializer.Serialize(SettingsModel, SettingsModelContext.Default.SettingsModel));
        }
    }
}