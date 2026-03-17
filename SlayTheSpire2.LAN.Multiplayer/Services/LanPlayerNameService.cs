using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Models;

// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class LanPlayerNameService
    {
        private static readonly Lazy<LanPlayerNameService> Lazy = new(() => new LanPlayerNameService());

        public static LanPlayerNameService Instance => Lazy.Value;

        public PlayerNames PlayerNames = GetDefaultPlayerNames();

        public INetGameService? NetService;

        public TaskCompletionSource<LanPlayerNameResponseMessage>? LanPlayerNameCompletion;

        private LanPlayerNameService()
        {
        }

        public void SetHostPlayerName()
        {
            if (!PlayerNameLineEdit.GetPlayerNameIsInvalid(SettingsService.Instance.SettingsModel.PlayerName))
            {
                PlayerNames[1u] = SettingsService.Instance.SettingsModel.PlayerName;
            }
            else
            {
                PlayerNames.Remove(1u);
            }
        }

        public void SetDefaultPlayerNames()
        {
            PlayerNames = GetDefaultPlayerNames();
        }

        private static PlayerNames GetDefaultPlayerNames()
        {
            return !PlayerNameLineEdit.GetPlayerNameIsInvalid(SettingsService.Instance.SettingsModel.PlayerName)
                ? new PlayerNames { { 1u, SettingsService.Instance.SettingsModel.PlayerName } }
                : new PlayerNames();
        }

        public void HandleLanPlayerNameResponseMessage(LanPlayerNameResponseMessage lanPlayerNameResponseMessage,
            ulong senderId)
        {
            PlayerNames = lanPlayerNameResponseMessage.playerNames;

            UpdatePlayerName();

            LanPlayerNameCompletion?.SetResult(lanPlayerNameResponseMessage);
        }

        public void HandleLanPlayerNameRequestMessage(LanPlayerNameRequestMessage lanPlayerNameRequestMessage,
            ulong senderId)
        {
            if (PlayerNameLineEdit.GetPlayerNameIsInvalid(lanPlayerNameRequestMessage.playerName))
            {
                NetService?.SendMessage(
                    new LanPlayerNameResponseMessage { playerNames = PlayerNames }, senderId);
                return;
            }

            PlayerNames[senderId] = lanPlayerNameRequestMessage.playerName;

            UpdatePlayerName();

            NetService?.SendMessage(new LanPlayerNameResponseMessage { playerNames = PlayerNames });
        }

        public async Task AttemptPlayerName(NetClientGameService gameService)
        {
            LanPlayerNameCompletion = new TaskCompletionSource<LanPlayerNameResponseMessage>();
            var message = new LanPlayerNameRequestMessage
                { playerName = SettingsService.Instance.SettingsModel.PlayerName };
            gameService.SendMessage(message);
            await LanPlayerNameCompletion.Task;
            LanPlayerNameCompletion = null;
        }

        private static void UpdatePlayerName()
        {
            var runScreenService = RunScreenService.Instance;

            if (runScreenService.CharacterSelectScreen != null)
            {
                UpdateNameplateLabel(runScreenService.CharacterSelectScreen);
            }

            if (runScreenService.DailyRunScreen != null)
            {
                UpdateNameplateLabel(runScreenService.DailyRunScreen);
            }

            if (runScreenService.CustomRunScreen != null)
            {
                UpdateNameplateLabel(runScreenService.CustomRunScreen);
            }

            if (runScreenService.MultiplayerLoadGameScreen != null)
            {
                UpdateNameplateLabel(runScreenService.MultiplayerLoadGameScreen);
            }

            if (runScreenService.DailyRunLoadScreen != null)
            {
                UpdateNameplateLabel(runScreenService.DailyRunLoadScreen);
            }

            if (runScreenService.CustomRunLoadScreen != null)
            {
                UpdateNameplateLabel(runScreenService.CustomRunLoadScreen);
            }
        }

        private static void UpdateNameplateLabel(Control container)
        {
            var nodes = Traverse.Create(container).Field("_remotePlayerContainer")
                .Field("_nodes").GetValue<List<NRemoteLobbyPlayer>>();

            foreach (var node in nodes)
            {
                var megaLabel = Traverse.Create(node).Field("_nameplateLabel").GetValue<MegaLabel>();

                if (!GodotObject.IsInstanceValid(megaLabel))
                    continue;

                megaLabel.SetTextAutoSize(PlatformUtil.GetPlayerName(PlatformType.None, node.PlayerId));
            }
        }
    }
}