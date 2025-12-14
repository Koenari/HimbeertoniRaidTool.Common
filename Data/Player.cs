using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Player : IHrtDataTypeWithId<Player>, ICloneable<Player>
{
    #region Static

    public static string DataTypeName => CommonLoc.DataTypeName_Player;

    public static HrtId.IdType IdType => HrtId.IdType.Player;

    public static Player Empty => new();

    #endregion

    #region Serialized

    [JsonProperty("Chars")] private readonly List<Reference<Character>> _characters = [];

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

    public IEnumerable<Character> Characters => _characters.Select(c => c.Data);

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
            return _characters[_mainCharIndex].Data;
        }
        set
        {
            if (_characters.Count > 0 && value.Equals(_characters[_mainCharIndex].Data)) return;
            _mainCharIndex = _characters.FindIndex(c => c.Data.Equals(value));
            if (_mainCharIndex >= 0)
                return;
            _characters.Add(value);
            _mainCharIndex = _characters.Count - 1;
        }
    }

    public string Name => NickName;

    IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    #endregion

    public Player Clone() => CloneService.Clone(this);

    public bool Equals(IHasHrtId? obj) => LocalId.Equals(obj?.LocalId);
    public override string ToString() => Name;

    public void RemoveCharacter(Character character)
    {
        var mainChar = MainChar;
        _characters.RemoveAll(c => c.Data.Equals(character));
        MainChar = mainChar;
    }
    public void AddCharacter(Character character) => _characters.Add(character);
}