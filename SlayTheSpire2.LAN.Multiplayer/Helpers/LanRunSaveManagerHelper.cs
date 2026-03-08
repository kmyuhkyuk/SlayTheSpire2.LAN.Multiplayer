using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Migrations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class LanRunSaveManagerHelper
    {
        public static ISaveStore SaveStore =>
            Traverse.Create(SaveManager.Instance).Field("_saveStore").GetValue<ISaveStore>();

        public static MigrationManager MigrationManager => Traverse.Create(SaveManager.Instance)
            .Field("_migrationManager").GetValue<MigrationManager>();

        public static IProfileIdProvider ProfileIdProvider => SaveManager.Instance;

        public static string CurrentMultiplayerRunSavePath =>
            RunSaveManager.GetRunSavePath(ProfileIdProvider.CurrentProfileId, "current_lan_run_mp.save");

        public static bool HasMultiplayerRunSave => SaveStore.FileExists(CurrentMultiplayerRunSavePath);

        public static ReadSaveResult<SerializableRun> LoadAndCanonicalizeMultiplayerRunSave(ulong localPlayerId)
        {
            var readSaveResult = LoadMultiplayerRunSave();
            if (readSaveResult is { Success: true, SaveData: not null })
            {
                try
                {
                    var data = RunManager.CanonicalizeSave(readSaveResult.SaveData, localPlayerId);
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

        public static ReadSaveResult<SerializableRun> LoadMultiplayerRunSave()
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

        public static void DeleteCurrentMultiplayerRun()
        {
            SaveStore.DeleteFile(CurrentMultiplayerRunSavePath);
        }

        public static void RenameBrokenMultiplayerRunSave(ReadSaveStatus status)
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