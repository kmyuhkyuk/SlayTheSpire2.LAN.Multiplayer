using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

// ReSharper disable InconsistentNaming

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    public struct ENetLanHandshakeResponse
    {
        public ENetHandshakeStatus status;

        public ulong netId;

        public ulong newNetId;
    }
}