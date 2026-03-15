using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Models;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch]
    internal class RunLobbyConstructorPatchs
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(StartRunLobby).GetConstructor([
                typeof(GameMode), typeof(INetGameService), typeof(IStartRunLobbyListener), typeof(int)
            ])!;
            yield return typeof(RunLobby).GetConstructor([
                typeof(GameMode), typeof(INetGameService), typeof(IRunLobbyListener), typeof(IPlayerCollection),
                typeof(IEnumerable<ulong>)
            ])!;
            yield return typeof(LoadRunLobby).GetConstructor([
                typeof(INetGameService), typeof(ILoadRunLobbyListener), typeof(SerializableRun)
            ])!;
        }

        private static void Prefix(INetGameService netService)
        {
            if (netService.Platform == PlatformType.None)
            {
                LanPlayerNameHelper.NetService = netService;

                netService.RegisterMessageHandler<LanPlayerNameResponseMessage>(LanPlayerNameHelper
                    .HandleLanPlayerNameResponseMessage);

                if (netService.Type == NetGameType.Host)
                {
                    netService.RegisterMessageHandler<LanPlayerNameRequestMessage>(LanPlayerNameHelper
                        .HandleLanPlayerNameRequestMessage);
                }
            }
        }
    }

    [HarmonyPatch]
    internal class RunLobbyCleanUpPatchs
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(StartRunLobby).GetMethod("CleanUp", BindingFlags.Instance | BindingFlags.Public)!;
            yield return typeof(RunLobby).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public)!;
            yield return typeof(LoadRunLobby).GetMethod("CleanUp", BindingFlags.Instance | BindingFlags.Public)!;
        }

        private static void Prefix(object __instance)
        {
            var netService = __instance is StartRunLobby or LoadRunLobby
                ? Traverse.Create(__instance).Property("NetService").GetValue<INetGameService>()
                : Traverse.Create(__instance).Field("_netService").GetValue<INetGameService>();

            if (netService.Platform == PlatformType.None)
            {
                LanPlayerNameHelper.SetDefaultPlayerNameDictionary();

                netService.UnregisterMessageHandler<LanPlayerNameResponseMessage>(LanPlayerNameHelper
                    .HandleLanPlayerNameResponseMessage);

                if (netService.Type == NetGameType.Host)
                {
                    netService.UnregisterMessageHandler<LanPlayerNameRequestMessage>(LanPlayerNameHelper
                        .HandleLanPlayerNameRequestMessage);
                }
            }
        }
    }
}