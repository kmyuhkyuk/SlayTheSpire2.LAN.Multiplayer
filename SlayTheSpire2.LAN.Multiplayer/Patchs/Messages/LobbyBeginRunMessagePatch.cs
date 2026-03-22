using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Messages
{
    [HarmonyPatch(typeof(LobbyBeginRunMessage), "Serialize")]
    internal class LobbyBeginRunMessageSerializePatch
    {
        private static bool Prefix(LobbyBeginRunMessage __instance, PacketWriter writer)
        {
            if (__instance.playersInLobby == null)
            {
                throw new InvalidOperationException("Tried to serialize ClientSlotGrantedMessage with null list!");
            }

            PacketHelper.WriteList(writer, __instance.playersInLobby);
            writer.WriteString(__instance.seed);
            writer.WriteList(__instance.modifiers);
            writer.WriteString(__instance.act1);

            return false;
        }
    }

    [HarmonyPatch(typeof(LobbyBeginRunMessage), "Deserialize")]
    internal class LobbyBeginRunMessageDeserializePatch
    {
        private static bool Prefix(ref LobbyBeginRunMessage __instance, PacketReader reader)
        {
            __instance.playersInLobby = PacketHelper.ReadList<LobbyPlayer>(reader);
            __instance.seed = reader.ReadString();
            __instance.modifiers = reader.ReadList<SerializableModifier>();
            __instance.act1 = reader.ReadString();

            return false;
        }
    }
}