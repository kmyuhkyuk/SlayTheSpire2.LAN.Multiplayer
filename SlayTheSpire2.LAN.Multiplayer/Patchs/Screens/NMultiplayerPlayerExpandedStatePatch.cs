using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Services;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NMultiplayerPlayerExpandedState), "_Ready")]
    internal class NMultiplayerPlayerExpandedStateReadyPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NMapDrawings), "GetDrawingStateForPlayer")]
        private static object GetDrawingStateForPlayer(NMapDrawings instance, ulong playerId)
        {
            throw new NotImplementedException();
        }

        private static void Prefix(NMultiplayerPlayerExpandedState __instance, Player ____player)
        {
            if (____player.NetId != RunManager.Instance.NetService.NetId)
            {
                var container = __instance.GetNode("ScreenContents/Container");

                var disableDrawing = PreloadManager.Cache
                    .GetScene(SceneHelper.GetScenePath("screens/card_library/card_library_tickbox"))
                    .Instantiate<Control>();

                if (disableDrawing is NLibraryStatTickbox cardLibraryTickBox &&
                    container.GetNode("MarginContainer") is MarginContainer marginContainer)
                {
                    disableDrawing.Name = "DisableDrawing";

                    container.AddChild(cardLibraryTickBox);
                    container.MoveChild(cardLibraryTickBox, 0);

                    marginContainer.RemoveThemeConstantOverride("margin_top");

                    cardLibraryTickBox.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

                    cardLibraryTickBox.SetLabel("Disable drawing");

                    var lanMapDrawingsService = LanMapDrawingsService.Instance;

                    cardLibraryTickBox.IsTicked =
                        lanMapDrawingsService.DisableDrawingHashSet.Contains(____player.NetId);

                    cardLibraryTickBox.Toggled += tickBox =>
                    {
                        if (NMapScreen.Instance == null)
                            return;

                        var drawingState = GetDrawingStateForPlayer(NMapScreen.Instance.Drawings, ____player.NetId);

                        var drawViewport = Traverse.Create(drawingState).Field("drawViewport").GetValue<SubViewport>();

                        if (tickBox.IsTicked)
                        {
                            if (drawViewport != null)
                            {
                                foreach (var line2D in drawViewport.GetChildren().OfType<Line2D>())
                                {
                                    line2D.Visible = false;
                                }
                            }

                            lanMapDrawingsService.DisableDrawingHashSet.Add(____player.NetId);
                        }
                        else
                        {
                            if (drawViewport != null)
                            {
                                foreach (var line2D in drawViewport.GetChildren().OfType<Line2D>())
                                {
                                    line2D.Visible = true;
                                }
                            }

                            lanMapDrawingsService.DisableDrawingHashSet.Remove(____player.NetId);
                        }
                    };
                }
            }
        }
    }
}