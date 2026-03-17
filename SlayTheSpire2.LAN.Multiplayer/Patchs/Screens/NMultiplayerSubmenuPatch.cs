using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Components;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Services;

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

                    var settingsModel = SettingsService.Instance.SettingsModel;

                    var stack = traverse.Field("_stack").GetValue<NSubmenuStack>();

                    if (SaveManager.Instance.Progress.NumberOfRuns > 0)
                    {
                        stack.PushSubmenuType<LanMultiplayerHostSubmenu>();
                    }
                    else
                    {
                        LanHostHelper.StartHost(GameMode.Standard,
                            traverse.Field("_loadingOverlay").GetValue<Control>(), stack, settingsModel.HostPort,
                            settingsModel.HostMaxPlayers);
                    }
                }));
            lanHostButton.SetIconAndLocalization("HOST");
            var lanHostTitle = Traverse.Create(lanHostButton).Field("_title").GetValue<MegaLabel>();
            lanHostTitle.Text = $"LAN {lanHostTitle.Text}";

            NSubmenuButtonDuplicateMaterial(lanHostButton);

            var lanMultiplayerSubmenuButtonService = LanMultiplayerSubmenuButtonService.Instance;

            lanMultiplayerSubmenuButtonService.LanHostButton = lanHostButton;

            if (buttonContainerNode.GetNode("LoadButton").Duplicate() is not NSubmenuButton lanLoadButton)
                return;

            buttonContainerNode.AddChild(lanLoadButton);
            buttonContainerNode.MoveChild(lanLoadButton, 2);

            lanLoadButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    var traverse = Traverse.Create(__instance);

                    var settingsModel = SettingsService.Instance.SettingsModel;

                    LanHostHelper.StartLoad(lanLoadButton, traverse.Field("_loadingOverlay").GetValue<Control>(),
                        traverse.Field("_stack").GetValue<NSubmenuStack>(),
                        settingsModel.HostPort, settingsModel.HostMaxPlayers);
                }));
            lanLoadButton.SetIconAndLocalization("MP_LOAD");
            var lanLoadButtonTitle = Traverse.Create(lanLoadButton).Field("_title").GetValue<MegaLabel>();
            lanLoadButtonTitle.Text = $"LAN {lanLoadButtonTitle.Text}";

            NSubmenuButtonDuplicateMaterial(lanLoadButton);

            lanMultiplayerSubmenuButtonService.LanLoadButton = lanLoadButton;

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

            lanMultiplayerSubmenuButtonService.LanAbandonButton = lanAbandonButton;
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
            var lanMultiplayerSubmenuButtonService = LanMultiplayerSubmenuButtonService.Instance;

            if (lanMultiplayerSubmenuButtonService.LanHostButton != null)
            {
                lanMultiplayerSubmenuButtonService.LanHostButton.Visible =
                    !LanRunSaveManagerService.Instance.HasMultiplayerRunSave;
            }

            if (lanMultiplayerSubmenuButtonService.LanLoadButton != null)
            {
                lanMultiplayerSubmenuButtonService.LanLoadButton.Visible =
                    LanRunSaveManagerService.Instance.HasMultiplayerRunSave;
            }

            if (lanMultiplayerSubmenuButtonService.LanAbandonButton != null)
            {
                lanMultiplayerSubmenuButtonService.LanAbandonButton.Visible =
                    LanRunSaveManagerService.Instance.HasMultiplayerRunSave;
            }
        }
    }
}