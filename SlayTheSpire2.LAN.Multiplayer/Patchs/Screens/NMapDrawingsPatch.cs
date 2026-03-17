using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
    internal class NMapDrawingsCreateLineForPlayerPatch
    {
        private static void Postfix(Player player, Line2D __result)
        {
            if (LanMapDrawingsService.Instance.DisableDrawingHashSet.Contains(player.NetId))
            {
                __result.Visible = false;
            }
        }
    }
}