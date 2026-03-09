using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(RunSaveManager), "SaveRun")]
    internal class RunSaveManagerSaveRunPatch
    {
        private static bool Prefix(RunSaveManager __instance, AbstractRoom? preFinishedRoom, bool ____forceSynchronous,
            ISaveStore ____saveStore, Action? ___Saved, ref Task __result)
        {
            if (!RunManager.Instance.ShouldSave || (RunManager.Instance.NetService.Type != NetGameType.Singleplayer &&
                                                    RunManager.Instance.NetService.Type != NetGameType.Host))
            {
                __result = Task.CompletedTask;
                return false;
            }

            __result = TaskHelper.RunSafely(SaveRun(__instance, preFinishedRoom, ____forceSynchronous, ____saveStore,
                ___Saved));

            return false;
        }

        private static async Task SaveRun(RunSaveManager runSaveManager, AbstractRoom? preFinishedRoom,
            bool forceSynchronous,
            ISaveStore saveStore, Action? saved)
        {
            var value = RunManager.Instance.ToSave(preFinishedRoom);
            var savePath = RunManager.Instance.NetService.Type.IsMultiplayer()
                ? RunManager.Instance.NetService.Platform == PlatformType.None
                    ? LanRunSaveManagerHelper.CurrentMultiplayerRunSavePath
                    : Traverse.Create(runSaveManager).Property("CurrentMultiplayerRunSavePath").GetValue<string>()
                : Traverse.Create(runSaveManager).Property("CurrentRunSavePath").GetValue<string>();
            using var stream = new MemoryStream();
            if (!forceSynchronous)
            {
                await JsonSerializer.SerializeAsync(stream, value,
                    JsonSerializationUtility.GetTypeInfo<SerializableRun>(), CancellationToken.None);
            }
            else
            {
                await JsonSerializer.SerializeAsync(stream, value,
                    JsonSerializationUtility.GetTypeInfo<SerializableRun>());
            }

            stream.Seek(0L, SeekOrigin.Begin);
            await saveStore.WriteFileAsync(savePath, stream.ToArray());

            saved?.Invoke();
        }
    }
}