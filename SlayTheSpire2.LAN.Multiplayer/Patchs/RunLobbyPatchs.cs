using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Models;
using SlayTheSpire2.LAN.Multiplayer.Services;

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
                var lanPlayerNameService = LanPlayerNameService.Instance;

                lanPlayerNameService.NetService = netService;

                netService.RegisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                    .HandleLanPlayerNameResponseMessage);

                if (netService.Type == NetGameType.Host)
                {
                    netService.RegisterMessageHandler<LanPlayerNameRequestMessage>(lanPlayerNameService
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
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            yield return typeof(StartRunLobby).GetMethod("CleanUp", flags)!;
            yield return typeof(LoadRunLobby).GetMethod("CleanUp", flags)!;
        }

        private static void Prefix(object __instance, bool disconnectSession)
        {
            var netService = Traverse.Create(__instance).Property("NetService").GetValue<INetGameService>();

            if (netService.Platform == PlatformType.None)
            {
                var lanPlayerNameService = LanPlayerNameService.Instance;

                if (disconnectSession)
                {
                    lanPlayerNameService.SetDefaultPlayerNames();
                }

                netService.UnregisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                    .HandleLanPlayerNameResponseMessage);

                if (netService.Type == NetGameType.Host)
                {
                    netService.UnregisterMessageHandler<LanPlayerNameRequestMessage>(lanPlayerNameService
                        .HandleLanPlayerNameRequestMessage);
                }
            }
        }

        [HarmonyPatch(typeof(RunLobby), "Dispose")]
        internal class RunLobbyDisposePatch
        {
            private static void Prefix(INetGameService ____netService)
            {
                if (____netService.Platform == PlatformType.None)
                {
                    var lanPlayerNameService = LanPlayerNameService.Instance;

                    lanPlayerNameService.SetDefaultPlayerNames();

                    ____netService.UnregisterMessageHandler<LanPlayerNameResponseMessage>(lanPlayerNameService
                        .HandleLanPlayerNameResponseMessage);

                    if (____netService.Type == NetGameType.Host)
                    {
                        ____netService.UnregisterMessageHandler<LanPlayerNameRequestMessage>(lanPlayerNameService
                            .HandleLanPlayerNameRequestMessage);
                    }
                }
            }
        }
    }
}