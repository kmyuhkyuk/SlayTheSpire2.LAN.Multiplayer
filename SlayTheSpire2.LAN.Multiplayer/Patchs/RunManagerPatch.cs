using HarmonyLib;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.Metrics;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    internal class RunManagerCleanUpPatch
    {
        private static void Postfix()
        {
            LanMapDrawingsService.Instance.DisableDrawingHashSet.Clear();
            LanPlayerNameService.Instance.SetDefaultPlayerNames();
        }
    }

    [HarmonyPatch(typeof(RunManager), "OnEnded")]
    internal class RunManagerOnEndedPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(RunManager), "UpdatePlayerStatsInMapPointHistory")]
        private static void UpdatePlayerStatsInMapPointHistory(RunManager instance)
        {
            throw new NotImplementedException();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(RunManager), "CheckUpdateEnemyDiscoveryAfterLoss")]
        private static void CheckUpdateEnemyDiscoveryAfterLoss(Player player, ModelId monster)
        {
            throw new NotImplementedException();
        }

        private static bool Prefix(RunManager __instance, bool isVictory, ref bool ____runHistoryWasUploaded,
            ref SerializableRun __result)
        {
            if (__instance.NetService.Platform == PlatformType.None)
            {
                UpdatePlayerStatsInMapPointHistory(__instance);
                var state = Traverse.Create(__instance).Property("State").GetValue<RunState?>();
                if (state == null)
                {
                    throw new Exception($"{nameof(state)} is null");
                }

                var me = LocalContext.GetMe(state);
                if (me == null)
                {
                    throw new Exception($"{nameof(me)} is null");
                }

                if (state is { CurrentRoom: CombatRoom combatRoom, CurrentMapPointHistoryEntry: not null })
                {
                    state.CurrentMapPointHistoryEntry.Rooms.Last().TurnsTaken = combatRoom.CombatState.RoundNumber;
                }

                var serializableRun = __instance.ToSave(null);
                var me2 = LocalContext.GetMe(serializableRun);
                if (me2 == null)
                {
                    throw new Exception($"{nameof(me2)} is null");
                }

                if (____runHistoryWasUploaded)
                {
                    __result = serializableRun;
                    return false;
                }

                ____runHistoryWasUploaded = true;
                if (!isVictory && state.CurrentRoom is CombatRoom combatRoom2)
                {
                    foreach (var monstersWithSlot in combatRoom2.Encounter.MonstersWithSlots)
                    {
                        var item = monstersWithSlot.Item1;
                        CheckUpdateEnemyDiscoveryAfterLoss(me, item.Id);
                    }
                }

                if (__instance.ShouldSave)
                {
                    using (SaveManager.Instance.BeginSaveBatch())
                    {
                        SaveManager.Instance.UpdateProgressWithRunData(serializableRun, isVictory);
                        foreach (var discoveredEpoch in me2.DiscoveredEpochs)
                        {
                            if (!me.DiscoveredEpochs.Contains(discoveredEpoch))
                            {
                                me.DiscoveredEpochs.Add(discoveredEpoch);
                            }
                        }

                        AchievementsHelper.AfterRunEnded(state, me, isVictory);
                        RunHistoryUtilities.CreateRunHistoryEntry(serializableRun, isVictory, __instance.IsAbandoned,
                            __instance.NetService.Platform);
                        MetricUtilities.UploadRunMetrics(serializableRun, isVictory, __instance.NetService.NetId);
                        if (SaveManager.Instance.Progress.NumberOfRuns == 5)
                        {
                            MetricUtilities.UploadSettingsMetric();
                        }

                        switch (__instance.NetService.Type)
                        {
                            case NetGameType.Singleplayer:
                                SaveManager.Instance.DeleteCurrentRun();
                                break;
                            case NetGameType.Host:
                                LanRunSaveManagerService.Instance.DeleteCurrentMultiplayerRun();
                                break;
                            case NetGameType.None:
                            case NetGameType.Client:
                            case NetGameType.Replay:
                            default:
                                break;
                        }
                    }

                    if (isVictory)
                    {
                        var score = ScoreUtility.CalculateScore(serializableRun, isVictory);
                        StatsManager.IncrementArchitectDamage(score);
                    }
                }

                if (__instance.DailyTime.HasValue)
                {
                    var type = __instance.NetService.Type;
                    if ((uint)(type - 1) <= 1u)
                    {
                        var score2 = ScoreUtility.CalculateScore(serializableRun, isVictory);
                        TaskHelper.RunSafely(DailyRunUtility.UploadScore(__instance.DailyTime.Value, score2,
                            serializableRun.Players));
                    }
                    else if (__instance.NetService.Type == NetGameType.Client)
                    {
                        TaskHelper.RunSafely(DailyRunUtility.UploadScore(__instance.DailyTime.Value, -99999,
                            serializableRun.Players));
                    }
                }

                __result = serializableRun;

                return false;
            }

            return true;
        }
    }
}