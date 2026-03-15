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
    internal class NMultiplayerLoadGameScreenInitializePatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return typeof(NMultiplayerLoadGameScreen).GetMethod("InitializeAsHost",
                BindingFlags.Instance | BindingFlags.Public)!;
            yield return typeof(NMultiplayerLoadGameScreen).GetMethod("InitializeAsClient",
                BindingFlags.Instance | BindingFlags.Public)!;
        }

        private static void Prefix(NMultiplayerLoadGameScreen __instance, INetGameService gameService)
        {
            if (gameService.Platform == PlatformType.None)
            {
                LanPlayerNameHelper.MultiplayerLoadGameScreen = __instance;
            }
        }
    }
}