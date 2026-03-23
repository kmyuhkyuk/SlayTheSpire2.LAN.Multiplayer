using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch("MegaCrit.Sts2.Core.Nodes.Screens.Overlays.NOverlayStack", "Remove")]
    internal class NOverlayStackRemovePatch
    {
        private static bool Prefix(object __instance, object screen)
        {
            var overlay = screen as GodotObject;
            if (overlay == null || !GodotObject.IsInstanceValid(overlay))
            {
                return false;
            }

            var connections = overlay.GetSignalConnectionList("Completed");
            foreach (var connection in connections)
            {
                if (connection["callable"].AsCallable().Target == __instance)
                {
                    GD.Print("NOverlayStack.Remove called twice, preventing crash.");
                    return false;
                }
            }

            return true;
        }
    }
}
