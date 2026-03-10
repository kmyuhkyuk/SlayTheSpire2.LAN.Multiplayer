using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
    internal class NRestSiteRoomPatch
    {
        private static void Prefix(NRestSiteRoom __instance, IRunState ____runState,
            List<Control> ____characterContainers)
        {
            if (____runState.Players.Count > 4)
            {
                var bgContainer = __instance.GetNode("BgContainer");

                var lastLeftCharacterUp = bgContainer.GetNode("Character_3");
                var lastLeftCharacterDown = bgContainer.GetNode("Character_1");

                var lastRightCharacterUp = bgContainer.GetNode("Character_4");
                var lastRightCharacterDown = bgContainer.GetNode("Character_2");

                for (var i = 4; i < ____runState.Players.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (i % 4 == 0)
                        {
                            AddCharacter(bgContainer, ref lastLeftCharacterUp, ____characterContainers, i, true);
                        }
                        else
                        {
                            AddCharacter(bgContainer, ref lastLeftCharacterDown, ____characterContainers, i, true);
                        }
                    }
                    else
                    {
                        if (i % 4 == 1)
                        {
                            AddCharacter(bgContainer, ref lastRightCharacterUp, ____characterContainers, i, false);
                        }
                        else
                        {
                            AddCharacter(bgContainer, ref lastRightCharacterDown, ____characterContainers, i, false);
                        }
                    }
                }
            }
        }

        private static void Postfix(NRestSiteRoom __instance, IRunState ____runState)
        {
            if (____runState.Players.Count > 4)
            {
                var bgContainer = __instance.GetNode("BgContainer");
                var restSiteBackground = bgContainer.GetChild(0);

                var lastLeftRestSite = restSiteBackground.GetNode("RestSiteLLog");
                var lastRightRestSite = restSiteBackground.GetNode("RestSiteRLog");

                for (var i = 4; i < ____runState.Players.Count; i++)
                {
                    if (i % 2 == 0 && i % 4 == 0)
                    {
                        AddRestSite(restSiteBackground, ref lastLeftRestSite, true);
                    }
                    else if (i % 4 == 1)
                    {
                        AddRestSite(restSiteBackground, ref lastRightRestSite, false);
                    }
                }
            }
        }

        private static void AddRestSite(Node restSiteBackground, ref Node restSite, bool isLeft)
        {
            var nextRestSiteIndex = restSite.GetIndex() + 1;
            restSite = restSite.Duplicate();
            restSiteBackground.AddChild(restSite);
            restSite.MoveChild(restSite, nextRestSiteIndex);

            if (restSite is Control control)
            {
                control.Position = new Vector2(isLeft ? control.Position.X - 200 : control.Position.X + 200,
                    control.Position.Y - 50);
            }
        }

        private static void AddCharacter(Node bgContainer, ref Node character, List<Control> characterContainers,
            int index, bool isLeft)
        {
            var nextCharacterIndex = character.GetIndex() + 1;
            character = character.Duplicate();
            bgContainer.AddChild(character);
            bgContainer.MoveChild(character, nextCharacterIndex);

            if (character is Control control)
            {
                control.Name = $"Character_{index + 1}";
                control.Position = new Vector2(isLeft ? control.Position.X - 200 : control.Position.X + 200,
                    control.Position.Y - 50);

                characterContainers.Add(control);
            }
        }
    }
}