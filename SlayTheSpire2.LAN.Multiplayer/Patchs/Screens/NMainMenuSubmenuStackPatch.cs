using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlayTheSpire2.LAN.Multiplayer.Components;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NMainMenuSubmenuStack), "GetSubmenuType", typeof(Type))]
    internal class NMainMenuSubmenuStackGetSubmenuTypePatch
    {
        private static bool Prefix(NMainMenuSubmenuStack __instance, Type type, ref NSubmenu __result)
        {
            if (type == typeof(LanMultiplayerHostSubmenu))
            {
                var lanMultiplayerHostSubmenu =
                    __instance.GetNodeOrNull<LanMultiplayerHostSubmenu>("LANMultiplayerHostSubmenu");

                if (lanMultiplayerHostSubmenu == null)
                {
                    lanMultiplayerHostSubmenu = LanMultiplayerHostSubmenu.Create();

                    if (lanMultiplayerHostSubmenu != null)
                    {
                        lanMultiplayerHostSubmenu.Visible = false;
                        __instance.AddChildSafely(lanMultiplayerHostSubmenu);

                        __result = lanMultiplayerHostSubmenu;

                        return false;
                    }
                }
                else
                {
                    __result = lanMultiplayerHostSubmenu;

                    return false;
                }
            }

            return true;
        }
    }
}