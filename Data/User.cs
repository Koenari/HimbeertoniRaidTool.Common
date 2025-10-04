using HimbeertoniRaidTool.Common.Data.Dto;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class User : IHrtDataTypeWithId<User, UserDto>, ICloneable<User>
{
    public static string DataTypeNameStatic => "user";
    public static HrtId.IdType IdTypeStatic => HrtId.IdType.User;


    #region Serialized

    [JsonProperty("Username")] public string Username { get; set; } = string.Empty;

    [JsonProperty("DisplayName")] public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("XIVAccounts")] private List<Reference<XivAccount>> _xivAccounts = [];

    [JsonProperty("Characters")] private List<Reference<Character>> _claimedCharacters = [];

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

    public IEnumerable<XivAccount> XivAccounts => _xivAccounts.Select(ac => ac.Data);

    public IEnumerable<Character> ClaimedCharacters => _claimedCharacters.Select(cc => cc.Data);

    public IEnumerable<Character> OwnedCharacters
    {
        get
        {
            foreach (var character in _claimedCharacters)
            {
                yield return character;
            }
            foreach (var character in _xivAccounts.SelectMany(xivAccount => xivAccount.Data.Characters))
            {
                yield return character;
            }
        }
    }

    public bool OwnsCharacter(Character character) => _claimedCharacters.Contains(character)
                                                   || _xivAccounts.Any(xivAccount =>
                                                                           xivAccount.Data.Characters.Contains(
                                                                               character));

    public User Clone() => CloneService.Clone(this);
    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);
    public UserDto ToDto() => new(this);
    public void UpdateFromDto(UserDto dto)
    {
        Username = dto.Username;
        DisplayName = dto.DisplayName;
    }
}