using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NJoinFriendScreen), "_Ready")]
    internal class NJoinFriendScreenReadyPatch
    {
        private static void Prefix(NJoinFriendScreen __instance)
        {
            var lanPanel = new NinePatchRect { Name = "LANPanel" };
            __instance.AddChild(lanPanel);

            lanPanel.PatchMarginTop = 12;
            lanPanel.PatchMarginBottom = 12;
            lanPanel.PatchMarginLeft = 12;
            lanPanel.PatchMarginRight = 12;

            lanPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            lanPanel.OffsetLeft = 450;
            lanPanel.OffsetTop = -338;
            lanPanel.OffsetRight = 790;
            lanPanel.OffsetBottom = 338;

            if (__instance.GetNode("Panel") is NinePatchRect panel)
            {
                lanPanel.Texture = panel.Texture;
                lanPanel.SelfModulate = panel.SelfModulate;
            }

            var vBoxContainer = new VBoxContainer();

            lanPanel.AddChild(vBoxContainer);

            vBoxContainer.Alignment = BoxContainer.AlignmentMode.Center;
            vBoxContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            vBoxContainer.AddThemeConstantOverride("separation", 24);

            if (__instance.GetNode("TitleLabel").Duplicate() is MegaLabel ipAddressLabel)
            {
                lanPanel.AddChild(ipAddressLabel);

                ipAddressLabel.SetTextAutoSize("LAN IP:");
                ipAddressLabel.CustomMinimumSize = new Vector2(300, 0);
                ipAddressLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            }

            var addressLineEdit = new AddressLineEdit { Name = "AddressInput" };

            vBoxContainer.AddChild(addressLineEdit);

            addressLineEdit.Text = SettingsService.Instance.SettingsModel.IPAddress;
            addressLineEdit.Alignment = HorizontalAlignment.Center;
            addressLineEdit.CustomMinimumSize = new Vector2(300, 50);
            addressLineEdit.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

            var joinButton = JoinButton.Create(__instance.GetNode<NJoinFriendRefreshButton>("RefreshButton"));

            joinButton.Name = "JointButton";

            vBoxContainer.AddChild(joinButton);

            joinButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ =>
            {
                var addressInfo = addressLineEdit.GetAddressInfo();

                if (!addressInfo.IsValid)
                    return;

                SettingsService.Instance.SettingsModel.IPAddress = addressLineEdit.Text;
                SettingsService.Instance.WriteSettings();

                ushort port = 33771;

                if (addressInfo.Port.HasValue)
                {
                    port = addressInfo.Port.Value;
                }

                DisplayServer.WindowSetTitle("Slay The Spire 2 (Client)");
                if (addressInfo.Address != null)
                {
                    TaskHelper.RunSafely(
                        __instance.JoinGameAsync(new ENetClientConnectionInitializer(
                            SettingsService.Instance.SettingsModel.NetId, addressInfo.Address, port)));
                }
            }));
        }
    }
}