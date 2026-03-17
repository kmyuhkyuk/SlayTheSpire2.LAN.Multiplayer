using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch]
    internal class RunScreenInitializePatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
        }

        private static void Prefix(object __instance, INetGameService gameService)
        {
            if (gameService.Platform == PlatformType.None)
            {
                var runScreenService = RunScreenService.Instance;

                switch (__instance)
                {
                    case NCharacterSelectScreen characterSelectScreen:
                        runScreenService.CharacterSelectScreen = characterSelectScreen;
                        break;
                    case NDailyRunScreen dailyRunScreen:
                        runScreenService.DailyRunScreen = dailyRunScreen;
                        break;
                    case NCustomRunScreen customRunScreen:
                        runScreenService.CustomRunScreen = customRunScreen;
                        break;
                }
            }
        }
    }
}