using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class IPAddressLineEdit : NMegaLineEdit
    {
        [GeneratedRegex(@"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$")]
        private static partial Regex IPRegex();

        [GeneratedRegex(
            @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.){3}(25[0-5]|(2[0-4]|1\d|[1-9]|)\d):((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([1-9][0-9]{0,3})|[0-9])$")]
        private static partial Regex IPAndPortRegex();

        [GeneratedRegex("^[0-9.:]*$")]
        private static partial Regex IPAllowRegex();

        private string _oldText = "";

        public bool IsOnlyIP => IPRegex().IsMatch(Text);

        public bool IsIPAndPort => IPAndPortRegex().IsMatch(Text);

        public string? IPAddress => IsOnlyIP ? Text : IsIPAndPort ? Text.Split(':').First() : null;

        public ushort? Port => IsIPAndPort ? Convert.ToUInt16(Text.Split(':').Last()) : null;

        public override void _Ready()
        {
            TextChanged += OnTextChanged;
            TextSubmitted += OnTextSubmitted;
        }

        private void OnTextChanged(string newText)
        {
            if (IPAllowRegex().IsMatch(newText))
            {
                _oldText = newText;
            }
            else
            {
                Text = _oldText;
                CaretColumn = Text.Length;
            }
        }

        private void OnTextSubmitted(string newText)
        {
            Modulate = Colors.White;

            if (IPRegex().IsMatch(newText))
                return;

            if (IPAndPortRegex().IsMatch(newText))
                return;

            Modulate = Colors.Red;
        }
    }
}