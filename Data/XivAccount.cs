using System.Security.Cryptography;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class XivAccount : IHrtDataTypeWithId
{
    public static string DataTypeNameStatic => "FFXIV account";
    public static HrtId.IdType IdTypeStatic => HrtId.IdType.XivAccount;
    public string DataTypeName => DataTypeNameStatic;
    [JsonProperty("Name")] public string Name { get; set; } = string.Empty;

    [JsonProperty("HashedId")] public ulong HashedId { get; set; }
    
    public HrtId.IdType IdType => IdTypeStatic;
    /// <summary>
    ///     HRT specific unique ID used for local storage.
    /// </summary>
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;
    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] private readonly List<HrtId> _remoteIds = [];
    
    public IList<HrtId> RemoteIds => _remoteIds;
    
    [JsonProperty("Characters")] private List<Character> _characters = [];
    
    public IList<Character> Characters => _characters;
    
    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);
    
    public static ulong HashId(ulong accountId)
    {
        if (accountId == 0)
            return 0;
        using var sha256 = SHA256.Create();
#pragma warning disable CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        //Static version crashes in wine
        var hash = sha256.ComputeHash(BitConverter.GetBytes(accountId));
#pragma warning restore CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        return BitConverter.ToUInt64(hash);
    }
}