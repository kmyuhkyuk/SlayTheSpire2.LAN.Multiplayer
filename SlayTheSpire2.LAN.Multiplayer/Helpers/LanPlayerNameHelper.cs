using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Platform;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Models;

// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class LanPlayerNameHelper
    {
        public static LanPlayerNames PlayerNameDictionary = DefaultPlayerNameDictionary;

        public static INetGameService? NetService;

        public static NCharacterSelectScreen? CharacterSelectScreen;

        public static NMultiplayerLoadGameScreen? MultiplayerLoadGameScreen;

        public static TaskCompletionSource<LanPlayerNameResponseMessage>? LanPlayerNameCompletion;

        private static LanPlayerNames DefaultPlayerNameDictionary =>
            !PlayerNameLineEdit.GetPlayerNameIsInvalid(SettingsHelper.Instance.SettingsModel.PlayerName)
                ? new LanPlayerNames
                {
                    { 1u, SettingsHelper.Instance.SettingsModel.PlayerName }
                }
                : new LanPlayerNames();

        public static void SetHostPlayerName()
        {
            if (!PlayerNameLineEdit.GetPlayerNameIsInvalid(SettingsHelper.Instance.SettingsModel.PlayerName))
            {
                PlayerNameDictionary[1u] = SettingsHelper.Instance.SettingsModel.PlayerName;
            }
            else
            {
                PlayerNameDictionary.Remove(1u);
            }
        }

        public static void SetDefaultPlayerNameDictionary()
        {
            PlayerNameDictionary = DefaultPlayerNameDictionary;
        }

        public static void HandleLanPlayerNameResponseMessage(LanPlayerNameResponseMessage lanPlayerNameResponseMessage,
            ulong senderId)
        {
            PlayerNameDictionary = lanPlayerNameResponseMessage.playerNameDictionary;

            UpdatePlayerName();

            LanPlayerNameCompletion?.SetResult(lanPlayerNameResponseMessage);
        }

        public static void HandleLanPlayerNameRequestMessage(LanPlayerNameRequestMessage lanPlayerNameRequestMessage,
            ulong senderId)
        {
            if (PlayerNameLineEdit.GetPlayerNameIsInvalid(lanPlayerNameRequestMessage.playerName))
            {
                NetService?.SendMessage(
                    new LanPlayerNameResponseMessage { playerNameDictionary = PlayerNameDictionary }, senderId);
                return;
            }

            PlayerNameDictionary[senderId] = lanPlayerNameRequestMessage.playerName;

            UpdatePlayerName();

            NetService?.SendMessage(new LanPlayerNameResponseMessage { playerNameDictionary = PlayerNameDictionary });
        }

        public static async Task AttemptPlayerName(NetClientGameService gameService)
        {
            LanPlayerNameCompletion = new TaskCompletionSource<LanPlayerNameResponseMessage>();
            var message = new LanPlayerNameRequestMessage
                { playerName = SettingsHelper.Instance.SettingsModel.PlayerName };
            gameService.SendMessage(message);
            await LanPlayerNameCompletion.Task;
            LanPlayerNameCompletion = null;
        }

        private static void UpdatePlayerName()
        {
            if (CharacterSelectScreen != null)
            {
                UpdateNameplateLabel(CharacterSelectScreen);
            }

            if (MultiplayerLoadGameScreen != null)
            {
                UpdateNameplateLabel(MultiplayerLoadGameScreen);
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