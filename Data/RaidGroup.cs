using System.Collections;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidGroup : IEnumerable<Player>, IHrtDataTypeWithId
{
    [JsonProperty("Members")] private readonly Player?[] _players;
    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();
    [JsonProperty("Name")] public string Name;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public RolePriority? RolePriority;
    [JsonProperty("TimeStamp")] public DateTime TimeStamp;
    [JsonProperty("Type")] public GroupType Type;
    [JsonProperty("TypeLocked")] public bool TypeLocked;

    public RaidGroup() : this("") { }

    public RaidGroup(string name) : this(name, GroupType.Raid) { }

    public RaidGroup(string name, GroupType type)
    {
        Type = type;
        TimeStamp = DateTime.Now;
        Name = name;
        _players = new Player[8];
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i] = new Player();
        }
    }

    private IEnumerable<Player> Players
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Filled)
                    yield return this[i];
            }
        }
    }

    public int Count => Type switch
    {
        GroupType.Solo  => 1,
        GroupType.Group => 4,
        GroupType.Raid  => 8,
        _               => throw new ArgumentOutOfRangeException(),
    };

    public Player this[int idx]
    {
        get
        {
            if (idx < 0 || idx >= Count)
                throw new IndexOutOfRangeException($"Raid group of type {Type} has no member at index {idx}");
            if (Type == GroupType.Group)
                idx *= 2;
            return _players[idx] ??= new Player();
        }
        set
        {
            if (idx < 0 || idx >= Count)
                throw new IndexOutOfRangeException($"Raid group of type {Type} has no member at index {idx}");
            if (Type == GroupType.Group)
                idx *= 2;
            _players[idx] = value;
        }
    }
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_RaidGroup;

    public IEnumerator<Player> GetEnumerator() => Players.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Players.GetEnumerator();
    string IHrtDataType.Name => Name;
    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Group;
    [JsonIgnore] public string DataTypeName => DataTypeNameStatic;
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;
    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);
    public override string ToString() => Name;

    public void SwapPlayers(int idx1, int idx2)
    {
        if (idx1 < 0 || idx2 < 0)
            throw new IndexOutOfRangeException(
                $"Raid group of type {Type} has no member at index {Math.Min(idx1, idx2)}");
        if (idx1 >= Count || idx2 >= Count)
            throw new IndexOutOfRangeException(
                $"Raid group of type {Type} has no member at index {Math.Max(idx1, idx2)}");
        if (Type == GroupType.Group)
            (idx1, idx2) = (idx1 * 2, idx2 * 2);
        (_players[idx1], _players[idx2]) = (_players[idx2], _players[idx1]);
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class Alliance
{

    [JsonConstructor]
    public Alliance(string name = "")
    {
        Name = name;
        TimeStamp = DateTime.Now;
        RaidGroups = new RaidGroup[3];
    }
    [JsonProperty("Name")] public string Name { get; set; }
    public DateTime TimeStamp { get; set; }
    [JsonProperty("Groups")] public RaidGroup[] RaidGroups { get; set; }
}