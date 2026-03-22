using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class PlayerNameLineEdit : NMegaLineEdit
    {
        [GeneratedRegex(@"[\x00-\x1F\x7F<>:""/\\|?*\x0B\x0C\x0D&;`#$%^+={}]")]
        private static partial Regex InvalidCharsRegex();

        public bool IsInvalid => GetPlayerNameIsInvalid(Text);

        public bool IsEmpty => string.IsNullOrEmpty(Text);

        public override void _Ready()
        {
            base._Ready();
            TextChanged += OnTextChanged;
        }

        private void OnTextChanged(string newText)
        {
            Modulate = Colors.White;

            if (string.IsNullOrEmpty(newText) || !GetPlayerNameIsInvalid(newText))
                return;

            Modulate = Colors.Red;
        }

        public static bool GetPlayerNameIsInvalid(string playerName)
        {
            return string.IsNullOrWhiteSpace(playerName) || playerName.StartsWith(' ') ||
                   InvalidCharsRegex().IsMatch(playerName);
        }
    }
}