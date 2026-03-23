using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Control = Godot.Control;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs.Screens
{
    [HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
    internal class NRestSiteRoomReadyPatch
    {
        private static void Prefix(NRestSiteRoom __instance, IRunState ____runState,
            List<Control> ____characterContainers)
        {
            if (____runState.Players.Count > 4)
            {
                var bgContainer = __instance.GetNode("BgContainer");

                var lastLeftCharacterUp = (Control)bgContainer.GetNode("Character_3");
                var lastLeftCharacterDown = (Control)bgContainer.GetNode("Character_1");

                var lastRightCharacterUp = (Control)bgContainer.GetNode("Character_4");
                var lastRightCharacterDown = (Control)bgContainer.GetNode("Character_2");

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

                var lastLeftRestSite = (Control)restSiteBackground.GetNode("RestSiteLLog");
                var lastRightRestSite = (Control)restSiteBackground.GetNode("RestSiteRLog");

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

        private static void AddRestSite(Node restSiteBackground, ref Control restSite, bool isLeft)
        {
            var nextRestSiteIndex = restSite.GetIndex() + 1;

            restSite = (Control)restSite.Duplicate();

            restSite.Position = new Vector2(isLeft ? restSite.Position.X - 200 : restSite.Position.X + 200,
                restSite.Position.Y - 50);

            restSiteBackground.AddChildSafely(restSite);
            restSite.MoveChild(restSite, nextRestSiteIndex);
        }

        private static void AddCharacter(Node bgContainer, ref Control character, List<Control> characterContainers,
            int index, bool isLeft)
        {
            var nextCharacterIndex = character.GetIndex() + 1;

            character = (Control)character.Duplicate();
            character.Name = $"Character_{index + 1}";
            bgContainer.AddChildSafely(character);
            bgContainer.MoveChild(character, nextCharacterIndex);
            character.Position = new Vector2(isLeft ? character.Position.X - 200 : character.Position.X + 200,
                character.Position.Y - 50);

            characterContainers.Add(character);
        }
    }
}