using HarmonyLib;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Messages
{
    [HarmonyPatch(typeof(ClientLobbyJoinResponseMessage), "Serialize")]
    internal class ClientLobbyJoinResponseMessageSerializePatch
    {
        private static bool Prefix(ClientLobbyJoinResponseMessage __instance, PacketWriter writer)
        {
            if (__instance.playersInLobby == null)
            {
                throw new InvalidOperationException("Tried to serialize ClientSlotGrantedMessage with null list!");
            }

            PacketHelper.WriteList(writer, __instance.playersInLobby);
            writer.WriteBool(__instance.dailyTime.HasValue);
            if (__instance.dailyTime.HasValue)
            {
                writer.Write(__instance.dailyTime.Value);
            }

            writer.WriteBool(__instance.seed != null);
            if (__instance.seed != null)
            {
                writer.WriteString(__instance.seed);
            }

            writer.WriteInt(__instance.ascension, 5);
            writer.WriteList(__instance.modifiers);

            return false;
        }
    }

    [HarmonyPatch(typeof(ClientLobbyJoinResponseMessage), "Deserialize")]
    internal class ClientLobbyJoinResponseMessageDeserializePatch
    {
        private static bool Prefix(ref ClientLobbyJoinResponseMessage __instance, PacketReader reader)
        {
            __instance.playersInLobby = PacketHelper.ReadList<LobbyPlayer>(reader);
            if (reader.ReadBool())
            {
                __instance.dailyTime = reader.Read<TimeServerResult>();
            }

            if (reader.ReadBool())
            {
                __instance.seed = reader.ReadString();
            }

            __instance.ascension = reader.ReadInt(5);
            __instance.modifiers = reader.ReadList<SerializableModifier>();
            return false;
        }
    }
}