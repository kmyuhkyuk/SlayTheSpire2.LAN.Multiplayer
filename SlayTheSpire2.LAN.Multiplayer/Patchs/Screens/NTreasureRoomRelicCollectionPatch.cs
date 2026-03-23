using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NTreasureRoomRelicCollection), "_Ready")]
    internal class NTreasureRoomRelicCollectionReadyPatch
    {
        private static void Prefix(NTreasureRoomRelicCollection __instance)
        {
            var runState = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState?>();

            if (runState is { Players.Count: > 4 })
            {
                var container = __instance.GetNode("Container");
                var multiplayerRelicHolder = container.GetNode("MultiplayerRelicHolder4");

                for (var i = 4; i < runState.Players.Count; i++)
                {
                    var nTreasureRoomRelicHolder = (NTreasureRoomRelicHolder)multiplayerRelicHolder.Duplicate();
                    nTreasureRoomRelicHolder.Name = $"MultiplayerRelicHolder{i + 1}";
                    container.AddChildSafely(nTreasureRoomRelicHolder);
                }
            }
        }

        private static void Postfix(NTreasureRoomRelicCollection __instance,
            List<NTreasureRoomRelicHolder> ____multiplayerHolders)
        {
            if (____multiplayerHolders.Count > 4)
            {
                var gridContainer = new GridContainer
                    { Name = "RelicContainer", Columns = 4, CustomMinimumSize = new Vector2(0, 469) };

                gridContainer.AddThemeConstantOverride("h_separation", 42);
                gridContainer.AddThemeConstantOverride("v_separation", 42);
                gridContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);

                var viewport = new Control { Name = "Viewport" };
                viewport.AddChildSafely(gridContainer);
                viewport.SetAnchorsPreset(Control.LayoutPreset.TopWide);
                viewport.OffsetLeft = 110;
                viewport.OffsetTop = 110;
                viewport.OffsetRight = -120;
                viewport.OffsetBottom = 580;

                var mask = new TextureRect { Name = "Mask", ClipContents = true };
                mask.AddChildSafely(viewport);
                mask.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
                mask.OffsetLeft = -450;
                mask.OffsetTop = 250;
                mask.OffsetRight = 450;
                mask.OffsetBottom = 830;

                var scrollbar = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("ui/scrollbar"))
                    .Instantiate<NScrollbar>();
                scrollbar.AnchorLeft = 0.794f;
                scrollbar.AnchorTop = 0.187f;
                scrollbar.AnchorRight = 0.818f;
                scrollbar.AnchorBottom = 0.947f;
                scrollbar.OffsetLeft = -200;
                scrollbar.OffsetTop = 158;
                scrollbar.OffsetRight = -199;
                scrollbar.OffsetBottom = -163;

                var scrollContainer = new NScrollableContainer { Name = "ScrollableContainer" };
                scrollContainer.AddChildSafely(mask);
                scrollContainer.AddChildSafely(scrollbar);

                scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
                scrollContainer.OffsetLeft = -510;
                scrollContainer.OffsetTop = -250;
                scrollContainer.OffsetRight = 510;
                scrollContainer.OffsetBottom = 250;

                var container = __instance.GetNode("Container");
                container.AddChildSafely(scrollContainer);

                scrollContainer.SetContent(gridContainer, 20, 30);

                //The animation will change MultiplayerHolder position. Manually fix the position.

                var position = new Vector2(0, 110);

                for (var i = 0; i < ____multiplayerHolders.Count; i++)
                {
                    if (i > 0)
                    {
                        position = i % 4 == 0
                            ? new Vector2(0, position.Y + 178)
                            : new Vector2(position.X + 178, position.Y);
                    }

                    var multiplayerHolder = ____multiplayerHolders[i];
                    multiplayerHolder.Position = position;
                    multiplayerHolder.Reparent(gridContainer, false);
                }
            }
        }
    }
}