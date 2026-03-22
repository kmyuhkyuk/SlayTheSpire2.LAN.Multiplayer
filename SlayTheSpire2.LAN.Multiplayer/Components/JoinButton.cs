using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class JoinButton : NJoinFriendRefreshButton
    {
        protected override string[] Hotkeys => [MegaInput.viewMap];

        public static JoinButton Create(NJoinFriendRefreshButton joinFriendRefreshButton)
        {
            var joinButton = new JoinButton();

            joinButton.CustomMinimumSize = new Vector2(150, 50);
            joinButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

            joinButton.MouseFilter = MouseFilterEnum.Stop;

            joinButton.Material = joinFriendRefreshButton.Material.Duplicate() as Material;

            var background = new NinePatchRect { Name = "Background" };
            joinButton.AddChild(background);

            background.MouseFilter = MouseFilterEnum.Ignore;

            background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            background.Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png");
            background.PatchMarginLeft = 12;
            background.PatchMarginTop = 12;
            background.PatchMarginRight = 12;
            background.PatchMarginBottom = 12;

            background.Modulate = joinFriendRefreshButton.SelfModulate;
            background.Material = joinButton.Material;

            foreach (var child in joinFriendRefreshButton.GetChildren())
            {
                joinButton.AddChild(child.Duplicate());
            }

            var controllerIcon = joinButton.GetNode<TextureRect>("ControllerIcon");

            controllerIcon.Owner = joinButton;

            controllerIcon.Position = new Vector2(controllerIcon.Position.X - 12, controllerIcon.Position.Y);

            return joinButton;
        }

        public override void _Ready()
        {
            base._Ready();

            var node = GetNode<MegaLabel>("Label");
            node.SetTextAutoSize(new LocString("main_menu_ui", "JOIN.title").GetFormattedText());
        }
    }
}