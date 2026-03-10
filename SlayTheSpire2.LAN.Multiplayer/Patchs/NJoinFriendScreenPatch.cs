using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NJoinFriendScreen), "_Ready")]
    internal class NJoinFriendScreenPatch
    {
        private static void Prefix(NJoinFriendScreen __instance)
        {
            var lanPanel = new NinePatchRect { Name = "LanPanel" };

            __instance.AddChild(lanPanel);

            lanPanel.PatchMarginTop = 12;
            lanPanel.PatchMarginBottom = 12;
            lanPanel.PatchMarginLeft = 12;
            lanPanel.PatchMarginRight = 12;

            lanPanel.AnchorLeft = 0.5f;
            lanPanel.AnchorTop = 0.5f;
            lanPanel.AnchorRight = 0.5f;
            lanPanel.AnchorBottom = 0.5f;

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
            vBoxContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vBoxContainer.Size = new Vector2(350, 676);
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

            addressLineEdit.Text = SettingsHelper.Instance.SettingsModel.IPAddress;
            addressLineEdit.Alignment = HorizontalAlignment.Center;
            addressLineEdit.CustomMinimumSize = new Vector2(300, 50);
            addressLineEdit.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

            if (__instance.GetNode<NJoinFriendRefreshButton>("RefreshButton").Duplicate() is NJoinFriendRefreshButton
                joinButton)
            {
                joinButton.Name = "JointButton";

                vBoxContainer.AddChild(joinButton);

                joinButton.CustomMinimumSize = new Vector2(150, 50);
                joinButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

                joinButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(_ =>
                {
                    var addressInfo = addressLineEdit.GetAddressInfo();

                    if (!addressInfo.IsValid)
                        return;

                    SettingsHelper.Instance.SettingsModel.IPAddress = addressLineEdit.Text;
                    SettingsHelper.Instance.WriteSettings();

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
                                SettingsHelper.Instance.SettingsModel.NetId, addressInfo.Address, port)));
                    }
                }));

                joinButton.Material = joinButton.Material.Duplicate() as Material;
                Traverse.Create(joinButton).Field("_hsv").SetValue(joinButton.Material);

                var joinButtonLabel = joinButton.GetNode<MegaLabel>("Label");

                joinButtonLabel.SetTextAutoSize(new LocString("main_menu_ui", "JOIN.title").GetFormattedText());
            }
        }
    }
}