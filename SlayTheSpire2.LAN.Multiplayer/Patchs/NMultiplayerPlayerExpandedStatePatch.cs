using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlayTheSpire2.LAN.Multiplayer.Helpers;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NMultiplayerPlayerExpandedState), "_Ready")]
    internal class NMultiplayerPlayerExpandedStatePatch
    {
        private static void Prefix(NMultiplayerPlayerExpandedState __instance, Player ____player)
        {
            if (____player.NetId != RunManager.Instance.NetService.NetId)
            {
                var lanPanel = new HBoxContainer { Name = "LANPanel" };
                __instance.AddChild(lanPanel);

                lanPanel.AnchorLeft = 0.5f;
                lanPanel.AnchorTop = 0;
                lanPanel.AnchorRight = 0.5f;
                lanPanel.AnchorBottom = 0;

                lanPanel.OffsetLeft = -162;
                lanPanel.OffsetTop = 100;
                lanPanel.OffsetRight = 162;
                lanPanel.OffsetBottom = 100;

                var disableDrawing = PreloadManager.Cache
                    .GetScene(SceneHelper.GetScenePath("screens/card_library/card_library_tickbox"))
                    .Instantiate<Control>();

                if (disableDrawing is NLibraryStatTickbox cardLibraryTickBox)
                {
                    disableDrawing.Name = "DisableDrawing";

                    lanPanel.AddChild(cardLibraryTickBox);

                    cardLibraryTickBox.SetLabel("Disable drawing");

                    cardLibraryTickBox.IsTicked = LanMapDrawingsHelper.DisableDrawingHashSet.Contains(____player.NetId);

                    cardLibraryTickBox.Toggled += tickBox =>
                    {
                        var drawingState = Traverse.Create(NMapScreen.Instance?.Drawings)
                            .Method("GetDrawingStateForPlayer", ____player.NetId).GetValue();
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

                            LanMapDrawingsHelper.DisableDrawingHashSet.Add(____player.NetId);
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

                            LanMapDrawingsHelper.DisableDrawingHashSet.Remove(____player.NetId);
                        }
                    };
                }
            }
        }
    }
}