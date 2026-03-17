using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    public struct LanPlayerNameResponseMessage : INetMessage
    {
        public PlayerNames playerNames;

        public bool ShouldBroadcast => false;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.Info;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteInt(playerNames.Count);
            foreach (var keyValue in playerNames)
            {
                writer.WriteULong(keyValue.Key);
                writer.WriteString(keyValue.Value);
            }
        }

        public void Deserialize(PacketReader reader)
        {
            var count = reader.ReadInt();
            playerNames = new PlayerNames();
            for (var i = 0; i < count; i++)
            {
                playerNames.Add(reader.ReadULong(), reader.ReadString());
            }
        }
    }
}