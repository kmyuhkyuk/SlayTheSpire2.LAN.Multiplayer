using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NTreasureRoomRelicCollection), "_Ready")]
    internal class NTreasureRoomRelicCollectionPatch
    {
        private static void Prefix(NTreasureRoomRelicCollection __instance)
        {
            var runState = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState?>();

            if (runState is { Players.Count: > 4 })
            {
                var container = __instance.GetNode("Container");
                var lastMultiplayerRelicHolder = container.GetNode("MultiplayerRelicHolder4");

                for (var i = 4; i < runState.Players.Count; i++)
                {
                    lastMultiplayerRelicHolder = lastMultiplayerRelicHolder.Duplicate();
                    container.AddChild(lastMultiplayerRelicHolder);

                    if (lastMultiplayerRelicHolder is NTreasureRoomRelicHolder nTreasureRoomRelicHolder)
                    {
                        nTreasureRoomRelicHolder.Name = $"MultiplayerRelicHolder{i + 1}";
                    }
                }
            }
        }

        private static void Postfix(List<NTreasureRoomRelicHolder> ____multiplayerHolders)
        {
            if (____multiplayerHolders.Count > 4)
            {
                var more16People = ____multiplayerHolders.Count > 16;

                var position = new Vector2(more16People ? 30 : 62, 110);

                for (var i = 0; i < ____multiplayerHolders.Count; i++)
                {
                    if (i > 0)
                    {
                        if (more16People)
                        {
                            position = i % 6 == 0
                                ? new Vector2(30, position.Y + 140)
                                : new Vector2(position.X + 140, position.Y);
                        }
                        else
                        {
                            position = i % 4 == 0
                                ? new Vector2(62, position.Y + 140)
                                : new Vector2(position.X + 240, position.Y);
                        }
                    }

                    ____multiplayerHolders[i].Position = position;
                }
            }
        }
    }
}