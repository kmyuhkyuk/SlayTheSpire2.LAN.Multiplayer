// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Services
{
    internal class LanMapDrawingsService
    {
        private static readonly Lazy<LanMapDrawingsService> Lazy = new(() => new LanMapDrawingsService());

        public static LanMapDrawingsService Instance => Lazy.Value;

        public readonly HashSet<ulong> DisableDrawingHashSet = [];

        private LanMapDrawingsService()
        {
        }
    }
}