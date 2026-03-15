using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    public struct LanPlayerNameResponseMessage : INetMessage
    {
        public LanPlayerNames playerNameDictionary;

        public bool ShouldBroadcast => false;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.Info;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteInt(playerNameDictionary.Count);
            foreach (var keyValue in playerNameDictionary)
            {
                writer.WriteULong(keyValue.Key);
                writer.WriteString(keyValue.Value);
            }
        }

        public void Deserialize(PacketReader reader)
        {
            var count = reader.ReadInt();
            playerNameDictionary = new LanPlayerNames();
            for (var i = 0; i < count; i++)
            {
                playerNameDictionary.Add(reader.ReadULong(), reader.ReadString());
            }
        }
    }
}