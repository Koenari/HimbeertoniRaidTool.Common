using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class User : IHrtDataTypeWithId<User>, ICloneable
{
    public static string DataTypeNameStatic => "user";
    public static HrtId.IdType IdTypeStatic => HrtId.IdType.User;


    #region Serialized

    [JsonProperty("Username")] public string Username { get; set; } = string.Empty;

    [JsonProperty("DisplayName")] public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("XIVAccounts")] private List<XivAccount> _xivAccounts = [];

    [JsonProperty("Characters")] private List<Character> _claimedCharacters = [];

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

    public string Name => DisplayName;

    public static HrtId.IdType IdType => IdTypeStatic;

    public IList<HrtId> RemoteIds => _remoteIds;

    public IList<XivAccount> XivAccounts => _xivAccounts;

    public IList<Character> ClaimedCharacters => _claimedCharacters;

    public IEnumerable<Character> OwnedCharacters
    {
        get
        {
            foreach (var character in _claimedCharacters)
            {
                yield return character;
            }
            foreach (var character in _xivAccounts.SelectMany(xivAccount => xivAccount.Characters))
            {
                yield return character;
            }
        }
    }

    public bool OwnsCharacter(Character character) => _claimedCharacters.Contains(character)
                                                   || _xivAccounts.Any(xivAccount =>
                                                                           xivAccount.Characters.Contains(character));


    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);
}