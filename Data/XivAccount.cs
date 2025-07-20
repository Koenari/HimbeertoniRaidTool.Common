using System.Security.Cryptography;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class XivAccount : IHrtDataTypeWithId<XivAccount>, ICloneable<XivAccount>
{
    public static string DataTypeNameStatic => "FFXIV account";
    public static HrtId.IdType IdTypeStatic => HrtId.IdType.XivAccount;

    private static SHA256 _sha256 = SHA256.Create();

    #region Serialized

    [JsonProperty("Name")] public string Name { get; set; } = string.Empty;

    [JsonProperty("HashedId")] public ulong HashedId { get; set; }

    [JsonProperty("Characters")] private List<Character> _characters = [];

    /// <summary>
    ///     HRT specific unique ID used for local storage.
    /// </summary>
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;
    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] private readonly List<HrtId> _remoteIds = [];

    #endregion

    public static string DataTypeName => DataTypeNameStatic;

    public static HrtId.IdType IdType => IdTypeStatic;


    public IList<HrtId> RemoteIds => _remoteIds;

    public IList<Character> Characters => _characters;

    public XivAccount Clone() => CloneService.Clone(this);
    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);

    public static ulong HashId(ulong accountId)
    {
        if (accountId == 0)
            return 0;
        //Static version crashes in wine
        byte[] hash = _sha256.ComputeHash(BitConverter.GetBytes(accountId));
        return BitConverter.ToUInt64(hash);
    }
}