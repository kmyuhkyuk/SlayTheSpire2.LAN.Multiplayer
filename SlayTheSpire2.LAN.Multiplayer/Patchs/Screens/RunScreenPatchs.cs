using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch]
    internal class RunScreenReadyPatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(NCharacterSelectScreen).GetMethod("_Ready", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("_Ready", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("_Ready", flags)!;
        }

        private static void Prefix(NSubmenu __instance)
        {
            var ipAddressInfoPanel = IPAddressInfoPanel.Create();
            ipAddressInfoPanel.Name = "IPAddressPanel";
            ipAddressInfoPanel.Visible = false;
            __instance.AddChildSafely(ipAddressInfoPanel);
        }
    }

    [HarmonyPatch]
    internal class RunScreenInitializeAsHostPatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("InitializeMultiplayerAsHost", flags)!;
        }

        private static void Prefix(NSubmenu __instance, INetGameService gameService)
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

        private static void Postfix(NSubmenu __instance, INetGameService gameService)
        {
            if (__instance.GetNode("IPAddressPanel") is IPAddressInfoPanel ipAddressInfoPanel)
            {
                if (gameService.Platform == PlatformType.None)
                {
                    ipAddressInfoPanel.Initialize();
                    ipAddressInfoPanel.Visible = true;
                }
                else
                {
                    ipAddressInfoPanel.Visible = false;
                }
            }
        }
    }

    [HarmonyPatch]
    internal class RunScreenInitializeAsClientPatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("InitializeMultiplayerAsClient", flags)!;
        }

        private static void Prefix(NSubmenu __instance, INetGameService gameService)
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

        private static void Postfix(NSubmenu __instance)
        {
            if (__instance.GetNode("IPAddressPanel") is IPAddressInfoPanel ipAddressInfoPanel)
            {
                ipAddressInfoPanel.Visible = false;
            }
        }
    }

    [HarmonyPatch]
    internal class RunScreenInitializeSingleplayerPatchs
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(NCharacterSelectScreen).GetMethod("InitializeSingleplayer", flags)!;
            yield return typeof(NDailyRunScreen).GetMethod("InitializeSingleplayer", flags)!;
            yield return typeof(NCustomRunScreen).GetMethod("InitializeSingleplayer", flags)!;
        }

        private static void Postfix(NSubmenu __instance)
        {
            if (__instance.GetNode("IPAddressPanel") is IPAddressInfoPanel ipAddressInfoPanel)
            {
                ipAddressInfoPanel.Visible = false;
            }
        }
    }
}