using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
    internal class NSettingsScreenPatch
    {
        private static void Prefix(NSettingsScreen __instance)
        {
            var moddingNode = __instance.GetNode("%Modding");

            var vBoxContainer = moddingNode.GetParent();

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect hostPortDivider &&
                moddingNode.Duplicate() is MarginContainer hostPort &&
                hostPort.GetNode("Label") is RichTextLabel hostPortLabel)
            {
                hostPortDivider.Name = "HostPortDivider";

                vBoxContainer.AddChild(hostPortDivider);
                vBoxContainer.MoveChild(hostPortDivider, moddingNode.GetIndex() + 1);

                hostPortDivider.Show();

                hostPort.Name = "HostPort";

                hostPort.RemoveChild(hostPort.GetNode("ModdingButton"));

                vBoxContainer.AddChild(hostPort);
                vBoxContainer.MoveChild(hostPort, hostPortDivider.GetIndex() + 1);

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

                vBoxContainer.AddChild(hostMaxPlayersDivider);
                vBoxContainer.MoveChild(hostMaxPlayersDivider, moddingNode.GetIndex() + 1);

                hostMaxPlayersDivider.Show();

                hostMaxPlayers.Name = "HostMaxPlayers";

                hostMaxPlayers.RemoveChild(hostMaxPlayers.GetNode("ModdingButton"));

                vBoxContainer.AddChild(hostMaxPlayers);
                vBoxContainer.MoveChild(hostMaxPlayers, hostMaxPlayersDivider.GetIndex() + 1);

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

            if (__instance.GetNode("%ModdingDivider").Duplicate() is ColorRect netIdDivider &&
                moddingNode.Duplicate() is MarginContainer netId &&
                netId.GetNode("Label") is RichTextLabel netIdLabel)
            {
                netIdDivider.Name = "NetIDDivider";

                vBoxContainer.AddChild(netIdDivider);
                vBoxContainer.MoveChild(netIdDivider, moddingNode.GetIndex() + 1);

                netIdDivider.Show();

                netId.Name = "NetID";

                netId.RemoveChild(netId.GetNode("ModdingButton"));

                vBoxContainer.AddChild(netId);
                vBoxContainer.MoveChild(netId, netIdDivider.GetIndex() + 1);

                netId.Show();

                var netIdInput = new SpinBox { Name = "NetIDInput" };

                netId.AddChild(netIdInput);

                netIdInput.CustomMinimumSize = new Vector2(324, 64);
                netIdInput.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                netIdInput.GetLineEdit().Alignment = HorizontalAlignment.Center;

                netIdInput.Step = 1;
                netIdInput.MinValue = 2;
                netIdInput.MaxValue = double.MaxValue;
                netIdInput.Value = SettingsHelper.Instance.SettingsModel.NetId;
                netIdInput.ValueChanged += value =>
                {
                    SettingsHelper.Instance.SettingsModel.NetId = (ulong)value;
                    SettingsHelper.Instance.WriteSettings();
                };

                netIdLabel.Text = "NetID";
            }
        }
    }
}