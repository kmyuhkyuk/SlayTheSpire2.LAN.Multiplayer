using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
    internal class NSettingsScreenReadyPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NSettingsPanel), "RefreshSize")]
        private static void RefreshSize(NSettingsPanel instance)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NSettingsScreen __instance)
        {
            var moddingNode = __instance.GetNode("%Modding");

            var vBoxContainerNode = moddingNode.GetParent();
            var generalSettings = vBoxContainerNode.GetParent();

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect hostPortDivider &&
                moddingNode.Duplicate() is MarginContainer hostPort &&
                hostPort.GetNode("Label") is MegaRichTextLabel hostPortLabel)
            {
                hostPortDivider.Name = "HostPortDivider";

                vBoxContainerNode.AddChild(hostPortDivider);
                vBoxContainerNode.MoveChild(hostPortDivider, moddingNode.GetIndex() + 1);

                hostPortDivider.Show();

                hostPort.Name = "HostPort";

                hostPort.RemoveChild(hostPort.GetNode("ModdingButton"));

                vBoxContainerNode.AddChild(hostPort);
                vBoxContainerNode.MoveChild(hostPort, hostPortDivider.GetIndex() + 1);

                hostPort.Show();

                var hostPortLineEdit = new SpinBox { Name = "HostPortInput" };
                hostPort.AddChild(hostPortLineEdit);

                hostPortLineEdit.CustomMinimumSize = new Vector2(324, 64);
                hostPortLineEdit.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                hostPortLineEdit.GetLineEdit().Alignment = HorizontalAlignment.Center;

                hostPortLineEdit.Step = 1;
                hostPortLineEdit.MinValue = 0;
                hostPortLineEdit.MaxValue = 65535;
                hostPortLineEdit.Value = SettingsService.Instance.SettingsModel.HostPort;
                hostPortLineEdit.ValueChanged += value =>
                {
                    SettingsService.Instance.SettingsModel.HostPort = (ushort)value;
                    SettingsService.Instance.WriteSettings();
                };

                hostPortLabel.Text = "Host Port";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect hostMaxPlayersDivider &&
                moddingNode.Duplicate() is MarginContainer hostMaxPlayers &&
                hostMaxPlayers.GetNode("Label") is MegaRichTextLabel hostMaxPlayersLabel)
            {
                hostMaxPlayersDivider.Name = "HostMaxPlayersDivider";

                vBoxContainerNode.AddChild(hostMaxPlayersDivider);
                vBoxContainerNode.MoveChild(hostMaxPlayersDivider, moddingNode.GetIndex() + 1);

                hostMaxPlayersDivider.Show();

                hostMaxPlayers.Name = "HostMaxPlayers";

                hostMaxPlayers.RemoveChild(hostMaxPlayers.GetNode("ModdingButton"));

                vBoxContainerNode.AddChild(hostMaxPlayers);
                vBoxContainerNode.MoveChild(hostMaxPlayers, hostMaxPlayersDivider.GetIndex() + 1);

                hostMaxPlayers.Show();

                var hostMaxPlayersInput = new SpinBox { Name = "HostMaxPlayersInput" };
                hostMaxPlayers.AddChild(hostMaxPlayersInput);

                hostMaxPlayersInput.CustomMinimumSize = new Vector2(324, 64);
                hostMaxPlayersInput.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                hostMaxPlayersInput.GetLineEdit().Alignment = HorizontalAlignment.Center;

                hostMaxPlayersInput.Step = 1;
                hostMaxPlayersInput.MinValue = 2;
                hostMaxPlayersInput.Value = SettingsService.Instance.SettingsModel.HostMaxPlayers;
                hostMaxPlayersInput.ValueChanged += value =>
                {
                    SettingsService.Instance.SettingsModel.HostMaxPlayers = (int)value;
                    SettingsService.Instance.WriteSettings();
                };

                hostMaxPlayersLabel.Text = "Host Max Players";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect playerNameDivider &&
                moddingNode.Duplicate() is MarginContainer playerName &&
                playerName.GetNode("Label") is MegaRichTextLabel playerNameLabel)
            {
                playerNameDivider.Name = "PlayerNameDivider";

                vBoxContainerNode.AddChild(playerNameDivider);
                vBoxContainerNode.MoveChild(playerNameDivider, moddingNode.GetIndex() + 1);

                playerNameDivider.Show();

                playerName.Name = "PlayerName";

                playerName.RemoveChild(playerName.GetNode("ModdingButton"));

                vBoxContainerNode.AddChild(playerName);
                vBoxContainerNode.MoveChild(playerName, playerNameDivider.GetIndex() + 1);

                playerName.Show();

                var marginContainer = new MarginContainer();
                playerName.AddChild(marginContainer);

                marginContainer.AddThemeConstantOverride("margin_right", 18);

                var playerNameInput = new PlayerNameLineEdit { Name = "PlayerNameInput" };
                marginContainer.AddChild(playerNameInput);

                playerNameInput.CustomMinimumSize = new Vector2(308, 64);
                playerNameInput.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                playerNameInput.Alignment = HorizontalAlignment.Center;

                playerNameInput.MaxLength = 16;
                playerNameInput.Text = SettingsService.Instance.SettingsModel.PlayerName;
                playerNameInput.TextChanged += value =>
                {
                    if (playerNameInput.IsEmpty || !playerNameInput.IsInvalid)
                    {
                        SettingsService.Instance.SettingsModel.PlayerName = value;
                        LanPlayerNameService.Instance.SetHostPlayerName();
                        SettingsService.Instance.WriteSettings();
                    }
                };

                playerNameLabel.Text = "Player Name";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect netIdDivider &&
                moddingNode.Duplicate() is MarginContainer netId &&
                netId.GetNode("Label") is MegaRichTextLabel netIdLabel)
            {
                netIdDivider.Name = "NetIDDivider";

                vBoxContainerNode.AddChild(netIdDivider);
                vBoxContainerNode.MoveChild(netIdDivider, moddingNode.GetIndex() + 1);

                netIdDivider.Show();

                netId.Name = "NetID";

                netId.RemoveChild(netId.GetNode("ModdingButton"));

                vBoxContainerNode.AddChild(netId);
                vBoxContainerNode.MoveChild(netId, netIdDivider.GetIndex() + 1);

                netId.Show();

                var netIdInput = new SpinBox { Name = "NetIDInput" };
                netId.AddChild(netIdInput);

                netIdInput.CustomMinimumSize = new Vector2(324, 64);
                netIdInput.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                netIdInput.GetLineEdit().Alignment = HorizontalAlignment.Center;

                netIdInput.Step = 1;
                netIdInput.MinValue = 2;
                netIdInput.MaxValue = ulong.MaxValue;
                netIdInput.Value = SettingsService.Instance.SettingsModel.NetId;
                netIdInput.ValueChanged += value =>
                {
                    SettingsService.Instance.SettingsModel.NetId = (ulong)value;
                    SettingsService.Instance.WriteSettings();
                };

                netIdLabel.Text = "NetID";
            }

            if (generalSettings is NSettingsPanel nSettingsPanel)
            {
                RefreshSize(nSettingsPanel);
            }
        }
    }

    [HarmonyPatch(typeof(NSettingsScreen), "LocalizeLabels")]
    internal class NSettingsScreenLocalizeLabelsPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NSettingsScreen), "LocHelper")]
        private static void LocHelper(Node settingsLineNode, LocString locString)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NSettingsScreen __instance)
        {
            var content = __instance.GetNode<NSettingsPanel>("%GeneralSettings").Content;

            LocHelper(content.GetNode<Node>("HostPort"),
                new LocString("settings_ui", "SlayTheSpire2.LAN.Multiplayer.HOST_PORT"));
            LocHelper(content.GetNode<Node>("HostMaxPlayers"),
                new LocString("settings_ui", "SlayTheSpire2.LAN.Multiplayer.HOST_MAX_PLAYERS"));
            LocHelper(content.GetNode<Node>("PlayerName"),
                new LocString("settings_ui", "SlayTheSpire2.LAN.Multiplayer.PLAYER_NAME"));
            LocHelper(content.GetNode<Node>("NetID"),
                new LocString("settings_ui", "SlayTheSpire2.LAN.Multiplayer.NET_ID"));
        }
    }
}