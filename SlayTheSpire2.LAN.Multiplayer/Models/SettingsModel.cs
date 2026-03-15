using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Saves;
using Steamworks;

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    [JsonSerializable(typeof(SettingsModel))]
    public partial class SettingsModelContext : JsonSerializerContext;

    public class SettingsModel : ISaveSchema
    {
        [JsonPropertyName("host_port")] public ushort HostPort { get; set; } = 33771;
        [JsonPropertyName("host_max_players")] public int HostMaxPlayers { get; set; } = 4;
        [JsonPropertyName("ip_address")] public string IPAddress { get; set; } = "127.0.0.1";
        [JsonPropertyName("net_id")] public ulong NetId { get; set; } = 1000u;
        [JsonPropertyName("player_name")] public string PlayerName { get; set; } = SteamFriends.GetPersonaName();

        public int SchemaVersion { get; set; }
    }
}