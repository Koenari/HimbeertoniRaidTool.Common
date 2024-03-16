using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Player : IHrtDataTypeWithId
{
    [JsonProperty("Chars")]
    private readonly List<Character> _characters = new();
    [JsonProperty("AdditionalData", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public readonly AdditionalPlayerData AdditionalData = new();

    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();
    [JsonProperty("MainCharIndex")]
    private int _mainCharIndex;

    [JsonProperty("NickName")]
    public string NickName = "";
    [JsonConstructor]
    public Player() { }
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
    [Obsolete("Use: MainChar.MainClass")] public PlayableClass? CurJob => MainChar.MainClass;
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_Player;
    public string Name => NickName;
    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Player;
    [JsonIgnore] public string DataTypeName => DataTypeNameStatic;

    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;
    public bool Equals(IHasHrtId? obj) => LocalId.Equals(obj?.LocalId);
    public override string ToString() => Name;

    public void RemoveCharacter(Character character)
    {
        Character mainChar = MainChar;
        _characters.Remove(character);
        MainChar = mainChar;
    }
    public void AddCharacter(Character character) => _characters.Add(character);
}

[JsonObject(MemberSerialization.OptIn)]
public class AdditionalPlayerData
{
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, float> FloatData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, int> IntData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, string> StringData = new();
}