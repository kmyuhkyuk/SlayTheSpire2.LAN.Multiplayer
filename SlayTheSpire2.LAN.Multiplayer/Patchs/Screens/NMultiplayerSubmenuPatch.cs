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
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch(typeof(NMultiplayerSubmenu), "UpdateButtons")]
        private static void UpdateButtons(NMultiplayerSubmenu instance)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NMultiplayerSubmenu __instance)
        {
            var buttonContainerNode = __instance.GetNode("ButtonContainer");

            if (buttonContainerNode.GetNode("HostButton").Duplicate() is NSubmenuButton lanHostButton)
            {
                NSubmenuButtonDuplicateMaterial(lanHostButton);

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

                LanMultiplayerSubmenuButtonService.Instance.LanHostButton = lanHostButton;
            }

            if (buttonContainerNode.GetNode("LoadButton").Duplicate() is NSubmenuButton lanLoadButton)
            {
                NSubmenuButtonDuplicateMaterial(lanLoadButton);

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

                LanMultiplayerSubmenuButtonService.Instance.LanLoadButton = lanLoadButton;
            }

            if (buttonContainerNode.GetNode("AbandonButton").Duplicate() is NSubmenuButton lanAbandonButton)
            {
                NSubmenuButtonDuplicateMaterial(lanAbandonButton);

                buttonContainerNode.AddChild(lanAbandonButton);
                buttonContainerNode.MoveChild(lanAbandonButton, 3);

                lanAbandonButton.Connect(NClickableControl.SignalName.Released,
                    Callable.From<NButton>(_ =>
                    {
                        TaskHelper.RunSafely(
                            LanHostHelper.TryAbandonMultiplayerRun(() => UpdateButtons(__instance)));
                    }));
                lanAbandonButton.SetIconAndLocalization("MP_ABANDON");
                var lanAbandonButtonTitle = Traverse.Create(lanAbandonButton).Field("_title").GetValue<MegaLabel>();
                lanAbandonButtonTitle.Text = $"LAN {lanAbandonButtonTitle.Text}";

                LanMultiplayerSubmenuButtonService.Instance.LanAbandonButton = lanAbandonButton;
            }
        }

        private static void NSubmenuButtonDuplicateMaterial(NSubmenuButton nSubmenuButton)
        {
            var bgPanel = nSubmenuButton.GetNode<Control>("BgPanel");
            bgPanel.Material = bgPanel.Material.Duplicate() as Material;
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