using System.Text.Json.Serialization;

namespace SlayTheSpire2.LAN.Multiplayer.Models
{
    [JsonSerializable(typeof(LanPlayerNames))]
    public partial class LanPlayerNamesContext : JsonSerializerContext;

    public class LanPlayerNames : Dictionary<ulong, string>;
}