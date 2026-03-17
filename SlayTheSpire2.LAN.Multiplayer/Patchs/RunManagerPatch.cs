using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    internal class RunManagerCleanUpPatch
    {
        private static void Postfix()
        {
            LanMapDrawingsService.Instance.DisableDrawingHashSet.Clear();
            LanPlayerNameService.Instance.SetDefaultPlayerNames();
        }
    }
}