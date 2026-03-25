using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Models;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(JoinFlow), "AttemptJoin")]
    internal class JoinFlowAttemptJoinPatch
    {
        private static void Postfix(JoinFlow __instance, ref Task<ClientLobbyJoinResponseMessage> __result)
        {
            __result = TaskGenericHelper.RunSafely(AttemptJoin(__instance, __result));
        }

        private static async Task<ClientLobbyJoinResponseMessage> AttemptJoin(JoinFlow joinFlow,
            Task<ClientLobbyJoinResponseMessage> clientLobbyJoinResponseMessage)
        {
            var result = await clientLobbyJoinResponseMessage;

            if (joinFlow.NetService is not { Platform: PlatformType.None })
                return result;

            var lanPlayerNameService = LanPlayerNameService.Instance;

            joinFlow.NetService.RegisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                .HandleLanPlayerNameResponseMessage);

            await lanPlayerNameService.AttemptPlayerName(joinFlow.NetService);

            return result;
        }
    }

    [HarmonyPatch(typeof(JoinFlow), "AttemptLoadJoin")]
    internal class JoinFlowAttemptLoadJoinPatch
    {
        private static void Postfix(JoinFlow __instance, ref Task<ClientLoadJoinResponseMessage> __result)
        {
            __result = TaskGenericHelper.RunSafely(AttemptLoadJoin(__instance, __result));
        }

        private static async Task<ClientLoadJoinResponseMessage> AttemptLoadJoin(JoinFlow joinFlow,
            Task<ClientLoadJoinResponseMessage> clientLoadJoinResponseMessage)
        {
            var result = await clientLoadJoinResponseMessage;

            if (joinFlow.NetService is not { Platform: PlatformType.None })
                return result;

            var lanPlayerNameService = LanPlayerNameService.Instance;

            joinFlow.NetService.RegisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                .HandleLanPlayerNameResponseMessage);

            await lanPlayerNameService.AttemptPlayerName(joinFlow.NetService);

            return result;
        }
    }

    [HarmonyPatch(typeof(JoinFlow), "AttemptRejoin")]
    internal class JoinFlowAttemptRejoinPatch
    {
        private static void Postfix(JoinFlow __instance, ref Task<ClientRejoinResponseMessage> __result)
        {
            __result = TaskGenericHelper.RunSafely(AttemptRejoin(__instance, __result));
        }

        private static async Task<ClientRejoinResponseMessage> AttemptRejoin(JoinFlow joinFlow,
            Task<ClientRejoinResponseMessage> clientRejoinResponseMessage)
        {
            var result = await clientRejoinResponseMessage;

            if (joinFlow.NetService is not { Platform: PlatformType.None })
                return result;

            var lanPlayerNameService = LanPlayerNameService.Instance;

            joinFlow.NetService.RegisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                .HandleLanPlayerNameResponseMessage);

            await lanPlayerNameService.AttemptPlayerName(joinFlow.NetService);

            return result;
        }
    }

    [HarmonyPatch(typeof(JoinFlow), "OnDisconnected")]
    internal class JoinFlowOnDisconnectedPatch
    {
        private static void Postfix(JoinFlow __instance, NetErrorInfo info)
        {
            if (__instance.NetService is { Platform: PlatformType.None })
            {
                var lanPlayerNameCompletion = LanPlayerNameService.Instance.LanPlayerNameCompletion;

                if (lanPlayerNameCompletion?.Task is { IsCompleted: false })
                {
                    var exception =
                        new ClientConnectionFailedException(
                            $"Unexpectedly disconnected from host while joining. Reason: {info.GetReason()}", info);

                    lanPlayerNameCompletion.SetException(exception);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JoinFlow), "Cancel")]
    internal class JoinFlowCancelPatch
    {
        private static void Postfix(JoinFlow __instance)
        {
            if (__instance.NetService is { Platform: PlatformType.None })
            {
                var lanPlayerNameCompletion = LanPlayerNameService.Instance.LanPlayerNameCompletion;

                if (lanPlayerNameCompletion?.Task is { IsCompleted: false })
                {
                    lanPlayerNameCompletion.SetCanceled();
                }
            }
        }
    }
}