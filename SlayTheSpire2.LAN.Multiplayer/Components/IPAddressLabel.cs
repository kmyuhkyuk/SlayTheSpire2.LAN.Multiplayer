using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class IPAddressLabel : MegaLabel
    {
        private string? _locKeyPrefix;

        public override void _Ready()
        {
            AutoSizeEnabled = false;

            MinFontSize = 24;

            AddThemeColorOverride("font_color", new Color(1.0f, 0.922f, 0.761f));
            AddThemeColorOverride("font_shadow_color", new Color(Colors.Black, 0.251f));

            var font = GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres");

            AddThemeFontOverride("font", font);
            AddThemeFontSizeOverride("font_size", 23);

            base._Ready();
        }

        public override void _Notification(int what)
        {
            if ((long)what == 2010 && IsNodeReady())
            {
                RefreshLabel();
            }

            base._Notification(what);
        }

        public void SetLocalization(string locKeyPrefix)
        {
            _locKeyPrefix = locKeyPrefix;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (_locKeyPrefix != null)
            {
                var locString = new LocString("main_menu_ui", _locKeyPrefix);
                SetTextAutoSize(locString.GetFormattedText());
                this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.Label.Font);
            }
        }
    }
}