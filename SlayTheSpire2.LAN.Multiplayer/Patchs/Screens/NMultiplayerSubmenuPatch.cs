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

            var lanHostButton = (NSubmenuButton)buttonContainerNode.GetNode("HostButton").Duplicate();

            NSubmenuButtonDuplicateMaterial(lanHostButton);

            buttonContainerNode.AddChildSafely(lanHostButton);
            buttonContainerNode.MoveChild(lanHostButton, 1);

            lanHostButton.SetIconAndLocalization("HOST");

            var lanHostTitle = Traverse.Create(lanHostButton).Field("_title").GetValue<MegaLabel>();
            lanHostTitle.SetTextAutoSize($"LAN {lanHostTitle.Text}");

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

            LanMultiplayerSubmenuButtonService.Instance.LanHostButton = lanHostButton;

            var lanLoadButton = (NSubmenuButton)buttonContainerNode.GetNode("LoadButton").Duplicate();

            NSubmenuButtonDuplicateMaterial(lanLoadButton);

            buttonContainerNode.AddChildSafely(lanLoadButton);
            buttonContainerNode.MoveChild(lanLoadButton, 2);

            lanLoadButton.SetIconAndLocalization("MP_LOAD");

            var lanLoadButtonTitle = Traverse.Create(lanLoadButton).Field("_title").GetValue<MegaLabel>();
            lanLoadButtonTitle.SetTextAutoSize($"LAN {lanLoadButtonTitle.Text}");

            lanLoadButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    var traverse = Traverse.Create(__instance);

                    var settingsModel = SettingsService.Instance.SettingsModel;

                    LanHostHelper.StartLoad(lanLoadButton, traverse.Field("_loadingOverlay").GetValue<Control>(),
                        traverse.Field("_stack").GetValue<NSubmenuStack>(),
                        settingsModel.HostPort, settingsModel.HostMaxPlayers);
                }));

            LanMultiplayerSubmenuButtonService.Instance.LanLoadButton = lanLoadButton;

            var lanAbandonButton = (NSubmenuButton)buttonContainerNode.GetNode("AbandonButton").Duplicate();

            NSubmenuButtonDuplicateMaterial(lanAbandonButton);

            buttonContainerNode.AddChildSafely(lanAbandonButton);
            buttonContainerNode.MoveChild(lanAbandonButton, 3);

            lanAbandonButton.SetIconAndLocalization("MP_ABANDON");

            var lanAbandonButtonTitle = Traverse.Create(lanAbandonButton).Field("_title").GetValue<MegaLabel>();
            lanAbandonButtonTitle.SetTextAutoSize($"LAN {lanAbandonButtonTitle.Text}");

            lanAbandonButton.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ =>
                {
                    TaskHelper.RunSafely(LanHostHelper.TryAbandonMultiplayerRun(__instance));
                }));

            LanMultiplayerSubmenuButtonService.Instance.LanAbandonButton = lanAbandonButton;
        }

        private static void NSubmenuButtonDuplicateMaterial(NSubmenuButton nSubmenuButton)
        {
            var bgPanel = nSubmenuButton.GetNode<Control>("BgPanel");
            bgPanel.Material = (Material)bgPanel.Material.Duplicate();
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