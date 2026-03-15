using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
    internal class NSettingsScreenReadyPatch
    {
        private static void Prefix(NSettingsScreen __instance)
        {
            var moddingNode = __instance.GetNode("%Modding");

            var vBoxContainerNode = moddingNode.GetParent();
            var generalSettings = vBoxContainerNode.GetParent();

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect hostPortDivider &&
                moddingNode.Duplicate() is MarginContainer hostPort &&
                hostPort.GetNode("Label") is RichTextLabel hostPortLabel)
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
                hostPortLineEdit.Value = SettingsHelper.Instance.SettingsModel.HostPort;
                hostPortLineEdit.ValueChanged += value =>
                {
                    SettingsHelper.Instance.SettingsModel.HostPort = (ushort)value;
                    SettingsHelper.Instance.WriteSettings();
                };

                hostPortLabel.Text = "Host Port";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect hostMaxPlayersDivider &&
                moddingNode.Duplicate() is MarginContainer hostMaxPlayers &&
                hostMaxPlayers.GetNode("Label") is RichTextLabel hostMaxPlayersLabel)
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
                hostMaxPlayersInput.Value = SettingsHelper.Instance.SettingsModel.HostMaxPlayers;
                hostMaxPlayersInput.ValueChanged += value =>
                {
                    SettingsHelper.Instance.SettingsModel.HostMaxPlayers = (int)value;
                    SettingsHelper.Instance.WriteSettings();
                };

                hostMaxPlayersLabel.Text = "Host Max Players";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect playerNameDivider &&
                moddingNode.Duplicate() is MarginContainer playerName &&
                playerName.GetNode("Label") is RichTextLabel playerNameLabel)
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

                var playerNameInput = new PlayerNameLineEdit { Name = "PlayerNameInput" };

                playerName.AddChild(playerNameInput);

                playerNameInput.CustomMinimumSize = new Vector2(324, 64);
                playerNameInput.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                playerNameInput.Alignment = HorizontalAlignment.Center;

                playerNameInput.MaxLength = 16;
                playerNameInput.Text = SettingsHelper.Instance.SettingsModel.PlayerName;
                playerNameInput.TextChanged += value =>
                {
                    if (playerNameInput.IsEmpty || !playerNameInput.IsInvalid)
                    {
                        SettingsHelper.Instance.SettingsModel.PlayerName = value;
                        LanPlayerNameHelper.SetHostPlayerName();
                        SettingsHelper.Instance.WriteSettings();
                    }
                };

                playerNameLabel.Text = "Player Name";
            }

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect netIdDivider &&
                moddingNode.Duplicate() is MarginContainer netId &&
                netId.GetNode("Label") is RichTextLabel netIdLabel)
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
                netIdInput.Value = SettingsHelper.Instance.SettingsModel.NetId;
                netIdInput.ValueChanged += value =>
                {
                    SettingsHelper.Instance.SettingsModel.NetId = (ulong)value;
                    SettingsHelper.Instance.WriteSettings();
                };

                netIdLabel.Text = "NetID";
            }

            if (generalSettings is NSettingsPanel nSettingsPanel)
            {
                Traverse.Create(nSettingsPanel).Method("RefreshSize").GetValue();
            }
        }
    }
}