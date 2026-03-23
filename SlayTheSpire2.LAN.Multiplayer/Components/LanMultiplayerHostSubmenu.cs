using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using SlayTheSpire2.LAN.Multiplayer.Helpers;
using SlayTheSpire2.LAN.Multiplayer.Services;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class LanMultiplayerHostSubmenu : NMultiplayerHostSubmenu
    {
        private static readonly string ScenePath = SceneHelper.GetScenePath("screens/multiplayer_host_submenu");

        private Control? _loadingOverlay;

        public new static LanMultiplayerHostSubmenu? Create()
        {
            if (TestMode.IsOn)
                return null;

            var multiplayerHostSubmenu =
                PreloadManager.Cache.GetScene(ScenePath).Instantiate<NMultiplayerHostSubmenu>();

            var lanMultiplayerHostSubmenu = new LanMultiplayerHostSubmenu();

            lanMultiplayerHostSubmenu.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            foreach (var child in multiplayerHostSubmenu.GetChildren())
            {
                child.Reparent(lanMultiplayerHostSubmenu, false);
            }

            multiplayerHostSubmenu.QueueFreeSafely();

            return lanMultiplayerHostSubmenu;
        }

        public override void _Ready()
        {
            ConnectSignals();

            var loadingOverlay = GetNode<Control>("LoadingOverlay");
            Traverse.Create(this).Field("_loadingOverlay").SetValue(loadingOverlay);
            _loadingOverlay = loadingOverlay;

            var standardButton = GetNode<NSubmenuButton>("StandardButton");
            Traverse.Create(this).Field("_standardButton").SetValue(standardButton);

            standardButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnStandardPressed));
            standardButton.SetIconAndLocalization("STANDARD_MP");

            var dailyButton = GetNode<NSubmenuButton>("DailyButton");
            Traverse.Create(this).Field("_dailyButton").SetValue(dailyButton);

            dailyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnDailyPressed));
            dailyButton.SetIconAndLocalization("DAILY_MP");

            var customButton = GetNode<NSubmenuButton>("CustomRunButton");
            Traverse.Create(this).Field("_customButton").SetValue(customButton);

            customButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCustomPressed));
            customButton.SetIconAndLocalization("CUSTOM_MP");
        }

        private void OnStandardPressed(NButton _)
        {
            StartHost(GameMode.Standard);
        }

        private void OnDailyPressed(NButton _)
        {
            StartHost(GameMode.Daily);
        }

        private void OnCustomPressed(NButton _)
        {
            StartHost(GameMode.Custom);
        }

        private new void StartHost(GameMode gameMode)
        {
            if (_loadingOverlay != null)
            {
                var settingsModel = SettingsService.Instance.SettingsModel;

                LanHostHelper.StartHost(gameMode, _loadingOverlay, _stack, settingsModel.HostPort,
                    settingsModel.HostMaxPlayers);
            }
        }
    }
}