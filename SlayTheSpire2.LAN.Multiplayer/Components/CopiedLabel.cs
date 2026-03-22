using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class CopiedLabel : MegaLabel
    {
        private Tween? _tween;

        private const string LocKeyPrefix = "SlayTheSpire2.LAN.Multiplayer.COPIED";

        public override void _Ready()
        {
            Modulate = new Color(Colors.White, 0);

            AutoSizeEnabled = false;

            MinFontSize = 24;

            AddThemeColorOverride("font_color", new Color(1.0f, 0.922f, 0.761f));
            AddThemeColorOverride("font_shadow_color", new Color(Colors.Black, 0.251f));

            var font = GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres");

            AddThemeFontOverride("font", font);
            AddThemeFontSizeOverride("font_size", 23);

            RefreshLabel();

            base._Ready();
        }

        public void ShowWithPosition(Vector2 position)
        {
            _tween?.Kill();

            Modulate = new Color(Colors.White);

            GlobalPosition = position;

            _tween = GetTree().CreateTween();

            _tween.SetParallel();

            _tween.TweenProperty(this, "position:y", Position.Y - 30, 0.3f).SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);

            _tween.Chain().TweenProperty(this, "modulate:a", 0, 0.4f).SetDelay(0.8f);
        }

        public override void _Notification(int what)
        {
            if ((long)what == 2010 && IsNodeReady())
            {
                RefreshLabel();
            }

            base._Notification(what);
        }

        private void RefreshLabel()
        {
            var locString = new LocString("main_menu_ui", LocKeyPrefix);
            SetTextAutoSize(locString.GetFormattedText());
            this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.Label.font);
        }
    }
}