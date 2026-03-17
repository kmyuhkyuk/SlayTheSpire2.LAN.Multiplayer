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
                if (LanMultiplayerHostSubmenu.Instance == null)
                {
                    var lanMultiplayerHostSubmenu = LanMultiplayerHostSubmenu.Create();

                    if (lanMultiplayerHostSubmenu != null)
                    {
                        lanMultiplayerHostSubmenu.Visible = false;
                        __instance.AddChildSafely(lanMultiplayerHostSubmenu);

                        __result = lanMultiplayerHostSubmenu;
                    }
                }
                else
                {
                    __result = LanMultiplayerHostSubmenu.Instance;
                }

                return false;
            }

            return true;
        }
    }
}