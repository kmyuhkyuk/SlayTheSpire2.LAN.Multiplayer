using System.Net;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal class AddressLineEdit : NMegaLineEdit
    {
        public override void _Ready()
        {
            TextSubmitted += OnTextSubmitted;
        }

        public struct AddressInfo(bool isValid, string? address, ushort? port)
        {
            public bool IsValid = isValid;
            public string? Address = address;
            public ushort? Port = port;
        }

        public AddressInfo GetAddressInfo()
        {
            var addressInfo = new AddressInfo();

            var uriString = Text;

            if (uriString == "localhost")
            {
                uriString = $"http://{uriString}";
            }

            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            {
                addressInfo.IsValid = true;
                addressInfo.Address = uri.Host;

                if (uri is { Port: > -1, IsDefaultPort: false })
                {
                    addressInfo.Port = (ushort)uri.Port;
                }

                return addressInfo;
            }

            if (IPAddress.TryParse(Text, out _))
            {
                addressInfo.IsValid = true;
                addressInfo.Address = Text;
            }
            else if (IPEndPoint.TryParse(Text, out var ipEndPoint))
            {
                addressInfo.IsValid = true;
                addressInfo.Port = (ushort)ipEndPoint.Port;
            }

            return addressInfo;
        }

        private void OnTextSubmitted(string newText)
        {
            Modulate = Colors.White;

            if (GetAddressInfo().IsValid)
                return;

            Modulate = Colors.Red;
        }
    }
}