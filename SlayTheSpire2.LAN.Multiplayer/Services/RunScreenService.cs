using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class RunScreenService
    {
        private static readonly Lazy<RunScreenService> Lazy = new(() => new RunScreenService());

        public static RunScreenService Instance => Lazy.Value;

        public NCharacterSelectScreen? CharacterSelectScreen;

        public NDailyRunScreen? DailyRunScreen;

        public NCustomRunScreen? CustomRunScreen;

        public NMultiplayerLoadGameScreen? MultiplayerLoadGameScreen;

        public NDailyRunLoadScreen? DailyRunLoadScreen;

        public NCustomRunLoadScreen? CustomRunLoadScreen;

        private RunScreenService()
        {
        }

        public static async Task<bool> ShouldAllowRunToBegin(LoadRunLobby runLobby)
        {
            if (runLobby.ConnectedPlayerIds.Count >= runLobby.Run.Players.Count)
                return true;

            var locString = new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.body");
            locString.Add("MissingCount", runLobby.Run.Players.Count - runLobby.ConnectedPlayerIds.Count);

            var nGenericPopup = NGenericPopup.Create();
            if (nGenericPopup != null)
            {
                NModalContainer.Instance?.Add(nGenericPopup);

                var nVerticalPopup = Traverse.Create(nGenericPopup).Field("_verticalPopup").GetValue<NVerticalPopup>();

                nVerticalPopup.NoButton.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
                nVerticalPopup.NoButton.OffsetLeft = -90;
                nVerticalPopup.NoButton.OffsetTop = -152;
                nVerticalPopup.NoButton.OffsetRight = 90;
                nVerticalPopup.NoButton.OffsetBottom = -80;

                nVerticalPopup.YesButton.Visible = false;

                return await nGenericPopup.WaitForConfirmation(locString,
                    new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.header"),
                    new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.cancel"),
                    new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.confirm"));
            }

            return false;
        }
    }
}