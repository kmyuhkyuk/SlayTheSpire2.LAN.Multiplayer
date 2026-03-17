using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
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
            __result = AttemptJoin(__instance, __result);
        }

        private static async Task<ClientLobbyJoinResponseMessage> AttemptJoin(JoinFlow joinFlow,
            Task<ClientLobbyJoinResponseMessage> clientLobbyJoinResponseMessage)
        {
            var result = await clientLobbyJoinResponseMessage;

            if (joinFlow.NetService == null)
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
            __result = AttemptLoadJoin(__instance, __result);
        }

        private static async Task<ClientLoadJoinResponseMessage> AttemptLoadJoin(JoinFlow joinFlow,
            Task<ClientLoadJoinResponseMessage> clientLoadJoinResponseMessage)
        {
            var result = await clientLoadJoinResponseMessage;

            if (joinFlow.NetService == null)
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
            __result = AttemptRejoin(__instance, __result);
        }

        private static async Task<ClientRejoinResponseMessage> AttemptRejoin(JoinFlow joinFlow,
            Task<ClientRejoinResponseMessage> clientRejoinResponseMessage)
        {
            var result = await clientRejoinResponseMessage;

            if (joinFlow.NetService == null)
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
        private static void Postfix(NetErrorInfo info)
        {
            var exception =
                new ClientConnectionFailedException(
                    $"Unexpectedly disconnected from host while joining. Reason: {info.GetReason()}", info);

            var lanPlayerNameCompletion = LanPlayerNameService.Instance.LanPlayerNameCompletion;

            if (lanPlayerNameCompletion?.Task is { IsCompleted: false })
            {
                lanPlayerNameCompletion.SetException(exception);
            }
        }
    }

    [HarmonyPatch(typeof(JoinFlow), "Cancel")]
    internal class JoinFlowCancelPatch
    {
        private static void Postfix()
        {
            var lanPlayerNameCompletion = LanPlayerNameService.Instance.LanPlayerNameCompletion;

            if (lanPlayerNameCompletion?.Task is { IsCompleted: false })
            {
                lanPlayerNameCompletion.SetCanceled();
            }
        }
    }
}