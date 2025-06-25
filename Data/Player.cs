using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Player : IHrtDataTypeWithId<Player>, ICloneable
{
    #region Static

    public static string DataTypeNameStatic => CommonLoc.DataTypeName_Player;

    #endregion

    #region Serialized

    [JsonProperty("Chars")] private readonly List<Character> _characters = [];

    [JsonProperty("MainCharIndex")] private int _mainCharIndex;

    [JsonProperty("NickName")] public string NickName = "";

    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = [];

    #endregion

    [JsonConstructor] public Player() { }

    #region Properties

    public IEnumerable<Character> Characters => _characters;

    public bool Filled => !LocalId.IsEmpty;

    public Character MainChar
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            // Deserialization Problems
            _characters.RemoveAll(c => c is null);
            if (_characters.Count == 0)
                _characters.Add(new Character());
            _mainCharIndex = Math.Clamp(_mainCharIndex, 0, _characters.Count - 1);
            return _characters[_mainCharIndex];
        }
        set
        {
            if (_characters.Count > 0 && value.Equals(_characters[_mainCharIndex])) return;
            _mainCharIndex = _characters.FindIndex(c => c.Equals(value));
            if (_mainCharIndex >= 0)
                return;
            _characters.Add(value);
            _mainCharIndex = _characters.Count - 1;
        }
    }

    public string Name => NickName;

    public HrtId.IdType IdType => HrtId.IdType.Player;

    public string DataTypeName => DataTypeNameStatic;

    IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    #endregion

    public bool Equals(IHasHrtId? obj) => LocalId.Equals(obj?.LocalId);
    public override string ToString() => Name;

    public void RemoveCharacter(Character character)
    {
        var mainChar = MainChar;
        _characters.Remove(character);
        MainChar = mainChar;
    }
    public void AddCharacter(Character character) => _characters.Add(character);
}