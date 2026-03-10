using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    internal class RunManagerPatch
    {
        private static void Postfix()
        {
            LanMapDrawingsHelper.DisableDrawingHashSet.Clear();
        }
    }
}