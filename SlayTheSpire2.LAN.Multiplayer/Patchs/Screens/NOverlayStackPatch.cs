using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NOverlayStack), "Remove")]
    internal class NOverlayStackRemovePatch
    {
        private static bool Prefix(NOverlayStack __instance, IOverlayScreen screen)
        {
            if (screen is not GodotObject overlay || !GodotObject.IsInstanceValid(overlay))
                return false;

            var connections = overlay.GetSignalConnectionList("Completed");

            foreach (var connection in connections)
            {
                if (connection["callable"].AsCallable().Target == __instance)
                {
                    Log.Debug("NOverlayStack.Remove called twice, preventing crash.");
                    return false;
                }
            }

            return true;
        }
    }
}