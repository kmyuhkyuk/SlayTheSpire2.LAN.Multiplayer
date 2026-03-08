using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;
using MegaCrit.Sts2.Core.Platform;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(ENetClientConnectionInitializer), "Connect")]
    internal class ENetClientConnectionInitializerPatch
    {
        private static bool Prefix(NetClientGameService gameService,
            CancellationToken cancelToken, ulong ____netId, string ____ip, ushort ____port, ref Task __result)
        {
            __result = TaskHelper.RunSafely(Connect(gameService, cancelToken, ____netId, ____ip, ____port));

            return false;
        }

        private static async Task<NetErrorInfo?> Connect(NetClientGameService gameService,
            CancellationToken cancelToken, ulong netId, string ip, ushort port)
        {
            if (gameService.IsConnected)
            {
                throw new InvalidOperationException(
                    "NetClientGameService must not be connected when passed to ENetClientConnectionInitializer!");
            }

            var eNetClient = new ENetClient(gameService);
            gameService.Initialize(eNetClient, PlatformType.None);

            var count = 0;
            const int tryCount = 10;
            NetErrorInfo? netErrorInfo = null;

            while (count < tryCount)
            {
                if (cancelToken.IsCancellationRequested)
                    return netErrorInfo;

                netErrorInfo = await eNetClient.ConnectToHost(netId, ip, port, cancelToken);

                if (!netErrorInfo.HasValue)
                {
                    Log.Info($"Connect {ip}:{port} Host Game NetID:{netId}");
                    return null;
                }

                if (netErrorInfo.Value.GetReason() != NetError.Kicked)
                    return netErrorInfo;

                var nextNetId = netId + 1000u;

                if (count < tryCount - 1)
                {
                    Log.Warn($"{ip}:{port} Host Game NetID:{netId} already occupied, Next will try NetID:{nextNetId}");
                }

                netId = nextNetId;

                count++;
            }

            return netErrorInfo;
        }
    }
}