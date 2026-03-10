using System.Buffers.Binary;
using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;
using SlayTheSpire2.LAN.Multiplayer.Models;

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class LanHandshakeResponseHelper
    {
        public static ENetPacket FromLanHandshakeResponse(ENetLanHandshakeResponse response)
        {
            var array = new byte[]
            {
                1,
                (byte)response.status,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
            var span = array.AsSpan();
            BinaryPrimitives.WriteUInt64BigEndian(span.Slice(2, 8), response.netId);
            BinaryPrimitives.WriteUInt64BigEndian(span.Slice(10, 8), response.newNetId);
            return new ENetPacket(array);
        }

        public static ENetLanHandshakeResponse AsLanHandshakeResponse(ENetPacket eNetPacket)
        {
            if (eNetPacket.PacketType != ENetPacketType.HandshakeResponse)
            {
                throw new InvalidOperationException(
                    $"Attempted to interpret ENet packet of type {eNetPacket.PacketType} as handshake response");
            }

            var span = eNetPacket.AllBytes.AsSpan();

            var status = (ENetHandshakeStatus)span[1];
            var netId = BinaryPrimitives.ReadUInt64BigEndian(span.Slice(2, 8));
            var newNetId = BinaryPrimitives.ReadUInt64BigEndian(span.Slice(10, 8));
            return new ENetLanHandshakeResponse
            {
                netId = netId,
                newNetId = newNetId,
                status = status
            };
        }
    }
}