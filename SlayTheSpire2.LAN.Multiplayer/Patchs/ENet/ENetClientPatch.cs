using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.ENet
{
    [HarmonyPatch(typeof(ENetClient), "ConnectToHost")]
    internal class ENetClientConnectToHostPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ENetClient), "HandleMessageReceived")]
        private static void HandleMessageReceived(ENetClient instance, ENetServiceData data)
        {
            throw new NotImplementedException();
        }

        private static bool Prefix(ENetClient __instance, ulong netId, string ip, ushort port,
            CancellationToken cancelToken, Logger ____logger, INetClientHandler ____handler, ref Task __result)
        {
            __result = TaskHelper.RunSafely(ConnectToHost(__instance, ____logger, ____handler, netId, ip, port,
                cancelToken));

            return false;
        }

        private static async Task<NetErrorInfo?> ConnectToHost(ENetClient eNetClient, Logger logger,
            INetClientHandler handler, ulong netId, string ip, ushort port, CancellationToken cancelToken)
        {
            while (true)
            {
                var connection = new ENetConnection();
                Traverse.Create(eNetClient).Field("_connection").SetValue(connection);
                connection.CreateHost();
                var peer = connection.ConnectToHost(ip, port);
                Traverse.Create(eNetClient).Field("_peer").SetValue(peer);
                var timeoutTimer = 0;
                while (!connection.TryService(out var output) ||
                       output is not { type: ENetConnection.EventType.Connect })
                {
                    await Task.Delay(100, cancelToken);
                    if (cancelToken.IsCancellationRequested)
                    {
                        eNetClient.DisconnectFromHost(NetError.CancelledJoin);
                        logger.Warn("User cancelled join flow");
                        return null;
                    }

                    timeoutTimer += 100;
                    if (timeoutTimer > 10000)
                    {
                        peer.Reset();
                        logger.Error("Connection timed out!");
                        return new NetErrorInfo(NetError.Timeout, selfInitiated: false);
                    }
                }

                if (peer.GetState() != ENetPacketPeer.PeerState.Connected)
                {
                    logger.Error($"Connection to {ip}:{port} failed!");
                    return new NetErrorInfo(NetError.UnknownNetworkError, selfInitiated: false);
                }

                var bufferedPackets = new List<ENetServiceData>();
                var (result, newNetId) = await SendAndWaitForNetIdAck(eNetClient, logger, peer, connection, netId,
                    bufferedPackets, cancelToken);
                if (result.HasValue)
                {
                    peer.PeerDisconnect();

                    if (result.Value.GetReason() == NetError.Kicked)
                    {
                        netId = newNetId;
                        continue;
                    }

                    return result;
                }

                Traverse.Create(eNetClient).Field("_netId").SetValue(newNetId);
                Traverse.Create(eNetClient).Field("_isConnected").SetValue(true);
                handler.OnConnectedToHost();
                foreach (var item in bufferedPackets)
                {
                    HandleMessageReceived(eNetClient, item);
                }

                return null;
            }
        }

        private static async Task<(NetErrorInfo?, ulong)> SendAndWaitForNetIdAck(ENetClient eNetClient, Logger logger,
            ENetPacketPeer peer, ENetConnection connection, ulong netId, List<ENetServiceData> bufferedPackets,
            CancellationToken cancelToken)
        {
            logger.Info($"Sending handshake with net ID {netId}");
            var eNetPacket = ENetPacket.FromHandshakeRequest(new ENetHandshakeRequest
            {
                netId = netId
            });
            peer.Send(0, eNetPacket.AllBytes, 1);
            var receivedAck = false;
            var timeoutTimer = 0;
            while (!receivedAck)
            {
                await Task.Delay(100, cancelToken);
                if (cancelToken.IsCancellationRequested)
                {
                    logger.Warn("User cancelled join flow");
                    eNetClient.DisconnectFromHost(NetError.CancelledJoin);
                    return (null, netId);
                }

                if (connection.TryService(out var output) && output is { type: ENetConnection.EventType.Receive })
                {
                    var packetData = output.Value.packetData;
                    var eNetPacket2 = new ENetPacket(packetData);
                    if (eNetPacket2.PacketType == ENetPacketType.ApplicationMessage)
                    {
                        bufferedPackets.Add(output.Value);
                        continue;
                    }

                    var eNetHandshakeResponse = LanHandshakeResponseHelper.AsLanHandshakeResponse(eNetPacket2);
                    if (eNetHandshakeResponse.netId != netId)
                    {
                        logger.Error(
                            $"Received net ID ({eNetHandshakeResponse.netId}) during handshake that did not match ours!");
                        return (new NetErrorInfo(NetError.InternalError, selfInitiated: false), netId);
                    }

                    if (eNetHandshakeResponse.status == ENetHandshakeStatus.IdCollision)
                    {
                        logger.Warn(
                            $"NetID:{netId} already occupied, Next try host send new NetID:{eNetHandshakeResponse.newNetId}");
                        return (new NetErrorInfo(NetError.Kicked, selfInitiated: false),
                            eNetHandshakeResponse.newNetId);
                    }
                    else if (eNetHandshakeResponse.status != ENetHandshakeStatus.Success)
                    {
                        logger.Error($"Received non-success code during handshake ({eNetHandshakeResponse.status})!");
                        return (new NetErrorInfo(NetError.Kicked, selfInitiated: false), netId);
                    }

                    receivedAck = true;
                }

                timeoutTimer += 100;
                if (timeoutTimer > 10000)
                {
                    logger.Error("Timed out waiting for handshake ack!");
                    eNetClient.DisconnectFromHost(NetError.Timeout);
                    return (new NetErrorInfo(NetError.Timeout, selfInitiated: false), netId);
                }
            }

            return (null, netId);
        }
    }
}