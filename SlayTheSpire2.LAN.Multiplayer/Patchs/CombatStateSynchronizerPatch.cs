using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(CombatStateSynchronizer), "WaitForSync")]
    internal class CombatStateSynchronizerWaitForSyncPatch
    {
        private static bool Prefix(CombatStateSynchronizer __instance, Logger ____logger,
            INetGameService ____netService, TaskCompletionSource? ____syncCompletionSource,
            Dictionary<ulong, SerializablePlayer> ____syncData, RunState ____runState, RunLobby? ____runLobby,
            SerializableRunRngSet? ____rngSet, SerializableRelicGrabBag? ____sharedRelicGrabBag, ref Task __result)
        {
            //Whether is LAN game was not checked, because the sync issue may also occur when connect via Steam

            __result = TaskHelper.RunSafely(WaitForSync(__instance, ____logger, ____netService,
                ____syncCompletionSource, ____syncData, ____runState, ____runLobby, ____rngSet,
                ____sharedRelicGrabBag));

            return false;
        }

        private static async Task WaitForSync(CombatStateSynchronizer instance, Logger logger,
            INetGameService netService, TaskCompletionSource? syncCompletionSource,
            Dictionary<ulong, SerializablePlayer> syncData, RunState runState, RunLobby? runLobby,
            SerializableRunRngSet? rngSet, SerializableRelicGrabBag? sharedRelicGrabBag)
        {
            logger.Debug("Waiting to receive all sync messages from all clients");
            if (netService.Type == NetGameType.Singleplayer || instance.IsDisabled)
                return;

            if (syncCompletionSource == null)
            {
                throw new InvalidOperationException("StartSync must be called before WaitForSync!");
            }

            var startTime = DateTime.Now;
            var lastResendTick = DateTime.Now;

            const int timeoutSeconds = 30;
            const int resendIntervalSeconds = 5;

            while (!syncCompletionSource.Task.IsCompleted)
            {
                if ((DateTime.Now - startTime).TotalSeconds > timeoutSeconds)
                {
                    logger.Warn("Receive all sync messages timeout, skip waiting for all clients");
                    break;
                }

                if (netService.Type == NetGameType.Host &&
                    (DateTime.Now - lastResendTick).TotalSeconds > resendIntervalSeconds && rngSet != null &&
                    sharedRelicGrabBag != null)
                {
                    logger.Debug("Resend rng sync message");

                    var message = new SyncRngMessage
                    {
                        rng = rngSet,
                        sharedRelicGrabBag = sharedRelicGrabBag
                    };

                    netService.SendMessage(message);

                    lastResendTick = DateTime.Now;
                }

                await Task.Delay(100);
            }

            foreach (var syncDatum in syncData)
            {
                if (runLobby != null && !runLobby.ConnectedPlayerIds.Contains(syncDatum.Key))
                {
                    logger.Debug($"Skipping sync for disconnected player {syncDatum.Key}");
                    continue;
                }

                var player = runState.GetPlayer(syncDatum.Key);
                if (!LocalContext.IsMe(player))
                {
                    player?.SyncWithSerializedPlayer(syncDatum.Value);
                }
            }

            if (netService.Type != NetGameType.Host)
            {
                if (rngSet != null)
                {
                    runState.Rng.LoadFromSerializable(rngSet);
                }
                else if (runState.Players.Count > 1)
                {
                    logger.Error(
                        "There are two or more players and we are a client, but we never received the RNG set!");
                }

                if (sharedRelicGrabBag != null)
                {
                    runState.SharedRelicGrabBag.LoadFromSerializable(sharedRelicGrabBag);
                }
                else if (runState.Players.Count > 1)
                {
                    logger.Error(
                        "There are two or more players and we are a client, but we never received the shared relic grab bag!");
                }
            }

            syncData.Clear();
            var traverse = Traverse.Create(instance);
            traverse.Field("_rngSet").SetValue(null);
            traverse.Field("_sharedRelicGrabBag").SetValue(null);
            traverse.Field("_syncCompletionSource").SetValue(null);
        }
    }
}