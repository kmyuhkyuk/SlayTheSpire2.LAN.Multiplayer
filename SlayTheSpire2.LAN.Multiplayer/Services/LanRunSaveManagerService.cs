using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Migrations;
using SlayTheSpire2.LAN.Multiplayer.Models;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class LanRunSaveManagerService
    {
        private static readonly Lazy<LanRunSaveManagerService> Lazy = new(() => new LanRunSaveManagerService());

        public static LanRunSaveManagerService Instance => Lazy.Value;

        private SaveManager ProfileIdProvider => SaveManager.Instance;

        private ISaveStore SaveStore => Traverse.Create(ProfileIdProvider).Field("_saveStore").GetValue<ISaveStore>();

        private MigrationManager MigrationManager => Traverse.Create(ProfileIdProvider).Field("_migrationManager")
            .GetValue<MigrationManager>();

        public string CurrentMultiplayerRunSavePath =>
            RunSaveManager.GetRunSavePath(ProfileIdProvider.CurrentProfileId, "current_lan_run_mp.save");

        public string CurrentMultiplayerRunPlayerNamesPath =>
            RunSaveManager.GetRunSavePath(ProfileIdProvider.CurrentProfileId, "current_lan_run_mp_player_names.json");

        public bool HasMultiplayerRunSave => SaveStore.FileExists(CurrentMultiplayerRunSavePath);

        private LanRunSaveManagerService()
        {
        }

        public ReadSaveResult<SerializableRun> LoadAndCanonicalizeMultiplayerRunSave(ulong localPlayerId)
        {
            var readSaveResult = LoadMultiplayerRunSave();
            if (readSaveResult is { Success: true, SaveData: not null })
            {
                try
                {
                    var lanPlayerNameService = LanPlayerNameService.Instance;

                    var data = RunManager.CanonicalizeSave(readSaveResult.SaveData, localPlayerId);
                    var playerNamesJson = SaveStore.ReadFile(CurrentMultiplayerRunPlayerNamesPath);
                    if (!string.IsNullOrEmpty(playerNamesJson))
                    {
                        lanPlayerNameService.PlayerNames = JsonSerializer.Deserialize<PlayerNames>(playerNamesJson) ??
                                                           new PlayerNames();
                    }

                    lanPlayerNameService.SetHostPlayerName();
                    return new ReadSaveResult<SerializableRun>(data, ReadSaveStatus.Success);
                }
                catch (Exception value)
                {
                    Log.Error($"Multiplayer run save validation failed: {value}");
                    RenameBrokenMultiplayerRunSave(ReadSaveStatus.ValidationFailed);
                    return new ReadSaveResult<SerializableRun>(ReadSaveStatus.ValidationFailed,
                        $"Save file validation failed: {value}");
                }
            }

            return readSaveResult;
        }

        public ReadSaveResult<SerializableRun> LoadMultiplayerRunSave()
        {
            var readSaveResult = MigrationManager.LoadSave<SerializableRun>(CurrentMultiplayerRunSavePath);
            if (readSaveResult.Success)
            {
                return readSaveResult;
            }

            if (readSaveResult.Status == ReadSaveStatus.FileNotFound)
            {
                Log.Info("Multiplayer run save file not found at " + CurrentMultiplayerRunSavePath);
            }
            else if (!readSaveResult.Status.IsRecoverable())
            {
                Log.Error(
                    $"Failed to load multiplayer run save: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
            }
            else
            {
                Log.Warn(
                    $"Multiplayer run save had recoverable issues: status={readSaveResult.Status} msg={readSaveResult.ErrorMessage}");
            }

            return readSaveResult;
        }

        public void DeleteCurrentMultiplayerRun()
        {
            SaveStore.DeleteFile(CurrentMultiplayerRunSavePath);
            SaveStore.DeleteFile(CurrentMultiplayerRunPlayerNamesPath);
        }

        public void RenameBrokenMultiplayerRunSave(ReadSaveStatus status)
        {
            try
            {
                if (HasMultiplayerRunSave)
                {
                    var text = CorruptFileHandler.GenerateCorruptFilePath(CurrentMultiplayerRunSavePath, status);
                    SaveStore.RenameFile(CurrentMultiplayerRunSavePath, text);
                    Log.Error(
                        $"Corrupt multiplayer run save detected: Renamed '{CurrentMultiplayerRunSavePath}' to '{text}'");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to rename broken multiplayer run save: " + ex.Message);
            }
        }
    }
}