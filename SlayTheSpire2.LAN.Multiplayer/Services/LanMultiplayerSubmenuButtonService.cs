using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class LanMultiplayerSubmenuButtonService
    {
        private static readonly Lazy<LanMultiplayerSubmenuButtonService> Lazy = new(() =>
            new LanMultiplayerSubmenuButtonService());

        public static LanMultiplayerSubmenuButtonService Instance => Lazy.Value;

        public NSubmenuButton? LanHostButton;
        public NSubmenuButton? LanLoadButton;
        public NSubmenuButton? LanAbandonButton;

        private LanMultiplayerSubmenuButtonService()
        {
        }
    }
}