using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch]
    internal class NCharacterSelectScreenInitializeMultiplayerPatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsClient",
                BindingFlags.Instance | BindingFlags.Public)!;
            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsHost",
                BindingFlags.Instance | BindingFlags.Public)!;
        }

        private static void Prefix(NCharacterSelectScreen __instance, INetGameService gameService)
        {
            if (gameService.Platform == PlatformType.None)
            {
                LanPlayerNameHelper.CharacterSelectScreen = __instance;
            }
        }
    }
}