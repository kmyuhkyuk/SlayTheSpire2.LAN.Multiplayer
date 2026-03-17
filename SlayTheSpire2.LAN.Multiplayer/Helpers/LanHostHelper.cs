using Godot;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class LanHostHelper
    {
        public static void StartHost(GameMode gameMode, Control loadingOverlay, NSubmenuStack stack, ushort port,
            int maxPlayers)
        {
            loadingOverlay.Visible = true;
            try
            {
                var netService = new NetHostGameService();
                NetErrorInfo? netErrorInfo = null;
                netService.StartENetHost(port, maxPlayers);
                Log.Info($"HostGame open on port:{port}");
                if (!netErrorInfo.HasValue)
                {
                    switch (gameMode)
                    {
                        case GameMode.Standard:
                        {
                            var submenuType3 = stack.GetSubmenuType<NCharacterSelectScreen>();
                            submenuType3.InitializeMultiplayerAsHost(netService, maxPlayers);
                            stack.Push(submenuType3);
                            break;
                        }
                        case GameMode.Daily:
                        {
                            var submenuType2 = stack.GetSubmenuType<NDailyRunScreen>();
                            submenuType2.InitializeMultiplayerAsHost(netService);
                            stack.Push(submenuType2);
                            break;
                        }
                        default:
                        {
                            var submenuType = stack.GetSubmenuType<NCustomRunScreen>();
                            submenuType.InitializeMultiplayerAsHost(netService, maxPlayers);
                            stack.Push(submenuType);
                            break;
                        }
                    }
                }
                else
                {
                    var nErrorPopup = NErrorPopup.Create(netErrorInfo.Value);
                    if (nErrorPopup != null)
                    {
                        NModalContainer.Instance?.Add(nErrorPopup);
                    }
                }
            }
            catch
            {
                var nErrorPopup2 = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
                if (nErrorPopup2 != null)
                {
                    NModalContainer.Instance?.Add(nErrorPopup2);
                }

                throw;
            }
            finally
            {
                loadingOverlay.Visible = false;
            }
        }

        public static void StartHost(SerializableRun run, Control loadingOverlay, NSubmenuStack stack, ushort port,
            int maxPlayers)
        {
            loadingOverlay.Visible = true;
            try
            {
                var netService = new NetHostGameService();
                NetErrorInfo? netErrorInfo = null;
                netService.StartENetHost(port, maxPlayers);
                Log.Info($"HostGame open on port:{port}");
                if (!netErrorInfo.HasValue)
                {
                    if (run.Modifiers.Count > 0)
                    {
                        if (run.DailyTime.HasValue)
                        {
                            var submenuType = stack.GetSubmenuType<NDailyRunLoadScreen>();
                            submenuType.InitializeAsHost(netService, run);
                            stack.Push(submenuType);
                        }
                        else
                        {
                            var submenuType2 = stack.GetSubmenuType<NCustomRunLoadScreen>();
                            submenuType2.InitializeAsHost(netService, run);
                            stack.Push(submenuType2);
                        }
                    }
                    else
                    {
                        var submenuType3 = stack.GetSubmenuType<NMultiplayerLoadGameScreen>();
                        submenuType3.InitializeAsHost(netService, run);
                        stack.Push(submenuType3);
                    }
                }
                else
                {
                    var nErrorPopup = NErrorPopup.Create(netErrorInfo.Value);
                    if (nErrorPopup != null)
                    {
                        NModalContainer.Instance?.Add(nErrorPopup);
                    }
                }
            }
            finally
            {
                loadingOverlay.Visible = false;
            }
        }

        public static void StartLoad(NSubmenuButton nSubmenuButton, Control loadingOverlay, NSubmenuStack stack,
            ushort port, int maxPlayers)
        {
            var readSaveResult =
                LanRunSaveManagerService.Instance.LoadAndCanonicalizeMultiplayerRunSave(
                    PlatformUtil.GetLocalPlayerId(PlatformType.None));
            if (!readSaveResult.Success || readSaveResult.SaveData == null)
            {
                Log.Warn("Broken multiplayer run save detected, disabling button");
                nSubmenuButton.Disable();
                var modalToCreate = NErrorPopup.Create(
                    new LocString("main_menu_ui", "INVALID_SAVE_POPUP.title"),
                    new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_run"),
                    new LocString("main_menu_ui", "INVALID_SAVE_POPUP.dismiss"), showReportBugButton: true);
                if (modalToCreate != null)
                {
                    NModalContainer.Instance?.Add(modalToCreate);
                }

                NModalContainer.Instance?.ShowBackstop();
            }
            else
            {
                StartHost(readSaveResult.SaveData, loadingOverlay, stack, port, maxPlayers);
            }
        }

        public static async Task TryAbandonMultiplayerRun(Action updateButtons)
        {
            var header = new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.header");
            var body = new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.body");
            var yesButton = new LocString("main_menu_ui", "GENERIC_POPUP.confirm");
            var noButton = new LocString("main_menu_ui", "GENERIC_POPUP.cancel");
            var nGenericPopup = NGenericPopup.Create();
            if (nGenericPopup != null)
            {
                NModalContainer.Instance?.Add(nGenericPopup);
            }

            if (nGenericPopup == null || !await nGenericPopup.WaitForConfirmation(body, header, noButton, yesButton))
                return;

            var readSaveResult =
                LanRunSaveManagerService.Instance.LoadAndCanonicalizeMultiplayerRunSave(
                    PlatformUtil.GetLocalPlayerId(PlatformType.None));
            if (readSaveResult is { Success: true, SaveData: not null })
            {
                try
                {
                    var saveData = readSaveResult.SaveData;
                    SaveManager.Instance.UpdateProgressWithRunData(saveData, victory: false);
                    RunHistoryUtilities.CreateRunHistoryEntry(saveData, victory: false, isAbandoned: true,
                        saveData.PlatformType);
                    if (saveData.DailyTime.HasValue)
                    {
                        PlatformUtil.GetLocalPlayerId(saveData.PlatformType);
                        var score = ScoreUtility.CalculateScore(saveData, won: false);
                        _ = TaskHelper.RunSafely(DailyRunUtility.UploadScore(saveData.DailyTime.Value, score,
                            saveData.Players));
                    }
                }
                catch (Exception value)
                {
                    Log.Error($"ERROR: Failed to upload run history/metrics: {value}");
                }
            }
            else
            {
                Log.Error(
                    $"ERROR: Failed to load multiplayer run save: status={readSaveResult.Status}. Deleting current run...");
            }

            LanRunSaveManagerService.Instance.DeleteCurrentMultiplayerRun();
            updateButtons();
        }
    }
}