using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NMultiplayerSubmenu), "_Ready")]
    internal class NMultiplayerSubmenuReadyPatch
    {
        private static void Prefix(NMultiplayerSubmenu __instance)
        {
            var buttonContainerNode = __instance.GetNode("ButtonContainer");

            if (buttonContainerNode.GetNode("HostButton").Duplicate() is not NSubmenuButton lanHostButton)
                return;

            buttonContainerNode.AddChild(lanHostButton);
            buttonContainerNode.MoveChild(lanHostButton, 1);

            lanHostButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    var traverse = Traverse.Create(__instance);

                    var settingsModel = SettingsHelper.Instance.SettingsModel;

                    LanHostHelper.StartHost(GameMode.Standard, traverse.Field("_loadingOverlay").GetValue<Control>(),
                        traverse.Field("_stack").GetValue<NSubmenuStack>(),
                        settingsModel.HostPort, settingsModel.HostMaxPlayers);
                }));
            lanHostButton.SetIconAndLocalization("HOST");
            var lanHostTitle = Traverse.Create(lanHostButton).Field("_title").GetValue<MegaLabel>();
            lanHostTitle.Text = $"LAN {lanHostTitle.Text}";

            NSubmenuButtonDuplicateMaterial(lanHostButton);

            NMultiplayerSubmenuButtonHelpers.LanHostButton = lanHostButton;

            if (buttonContainerNode.GetNode("LoadButton").Duplicate() is not NSubmenuButton lanLoadButton)
                return;

            buttonContainerNode.AddChild(lanLoadButton);
            buttonContainerNode.MoveChild(lanLoadButton, 2);

            lanLoadButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    var traverse = Traverse.Create(__instance);

                    var settingsModel = SettingsHelper.Instance.SettingsModel;

                    LanHostHelper.StartLoad(lanLoadButton, traverse.Field("_loadingOverlay").GetValue<Control>(),
                        traverse.Field("_stack").GetValue<NSubmenuStack>(),
                        settingsModel.HostPort, settingsModel.HostMaxPlayers);
                }));
            lanLoadButton.SetIconAndLocalization("MP_LOAD");
            var lanLoadButtonTitle = Traverse.Create(lanLoadButton).Field("_title").GetValue<MegaLabel>();
            lanLoadButtonTitle.Text = $"LAN {lanLoadButtonTitle.Text}";

            NSubmenuButtonDuplicateMaterial(lanLoadButton);

            NMultiplayerSubmenuButtonHelpers.LanLoadButton = lanLoadButton;

            if (buttonContainerNode.GetNode("AbandonButton").Duplicate() is not NSubmenuButton lanAbandonButton)
                return;

            buttonContainerNode.AddChild(lanAbandonButton);
            buttonContainerNode.MoveChild(lanAbandonButton, 3);

            lanAbandonButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    var traverse = Traverse.Create(__instance);

                    TaskHelper.RunSafely(
                        LanHostHelper.TryAbandonMultiplayerRun(() => traverse.Method("UpdateButtons").GetValue()));
                }));
            lanAbandonButton.SetIconAndLocalization("MP_ABANDON");
            var lanAbandonButtonTitle = Traverse.Create(lanAbandonButton).Field("_title").GetValue<MegaLabel>();
            lanAbandonButtonTitle.Text = $"LAN {lanAbandonButtonTitle.Text}";

            NSubmenuButtonDuplicateMaterial(lanAbandonButton);

            NMultiplayerSubmenuButtonHelpers.LanAbandonButton = lanAbandonButton;
        }

        private static void NSubmenuButtonDuplicateMaterial(NSubmenuButton nSubmenuButton)
        {
            var traverse = Traverse.Create(nSubmenuButton);

            var bgPanel = traverse.Field("_bgPanel").GetValue<Control>();
            bgPanel.Material = bgPanel.Material.Duplicate() as Material;

            traverse.Field("_hsv").SetValue(bgPanel.Material);
        }
    }

    [HarmonyPatch(typeof(NMultiplayerSubmenu), "UpdateButtons")]
    internal class NMultiplayerSubmenuUpdateButtonsPatch
    {
        private static void Postfix()
        {
            if (NMultiplayerSubmenuButtonHelpers.LanHostButton != null)
            {
                NMultiplayerSubmenuButtonHelpers.LanHostButton.Visible = !LanRunSaveManagerHelper.HasMultiplayerRunSave;
            }

            if (NMultiplayerSubmenuButtonHelpers.LanLoadButton != null)
            {
                NMultiplayerSubmenuButtonHelpers.LanLoadButton.Visible = LanRunSaveManagerHelper.HasMultiplayerRunSave;
            }

            if (NMultiplayerSubmenuButtonHelpers.LanAbandonButton != null)
            {
                NMultiplayerSubmenuButtonHelpers.LanAbandonButton.Visible =
                    LanRunSaveManagerHelper.HasMultiplayerRunSave;
            }
        }
    }
}