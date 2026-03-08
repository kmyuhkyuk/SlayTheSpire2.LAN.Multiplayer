using System.Text.Json;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Models;

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class SettingsHelper
    {
        private static readonly Lazy<SettingsHelper> Lazy = new(() => new SettingsHelper());

        public static SettingsHelper Instance => Lazy.Value;

        public readonly SettingsModel SettingsModel;

        private readonly GodotFileIo _modsDir =
            new(Path.Combine(UserDataPathProvider.GetAccountScopedBasePath(null), "mods"));

        private SettingsHelper()
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