using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    public struct LanPlayerNameRequestMessage : INetMessage
    {
        public string playerName;

        public bool ShouldBroadcast => false;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.Info;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteString(playerName);
        }

        public void Deserialize(PacketReader reader)
        {
            playerName = reader.ReadString();
        }
    }
}