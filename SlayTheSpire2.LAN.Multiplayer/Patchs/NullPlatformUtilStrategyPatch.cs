using HarmonyLib;
using MegaCrit.Sts2.Core.Platform.Null;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NullPlatformUtilStrategy), "GetPlayerName")]
    internal class NullPlatformUtilStrategyGetPlayerNamePatch
    {
        private static bool Prefix(ulong playerId, List<NullMultiplayerName>? ____mpNames, ref string __result)
        {
            if (____mpNames != null)
            {
                foreach (var mpName in ____mpNames)
                {
                    if (mpName.netId == playerId)
                    {
                        __result = mpName.name;
                        return false;
                    }
                }
            }

            if (LanPlayerNameHelper.PlayerNameDictionary.TryGetValue(playerId, out var playerName))
            {
                __result = playerName;
                return false;
            }

            __result = playerId switch
            {
                1uL => "Test Host",
                1000uL => "Test Client 1",
                2000uL => "Test Client 2",
                3000uL => "Test Client 3",
                _ => playerId.ToString(),
            };

            return false;
        }
    }
}