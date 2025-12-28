using System.Collections;
using HimbeertoniRaidTool.Common.Data.Dto;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidGroup : IEnumerable<Player>, IHrtDataTypeWithDto<RaidGroup, RaidGroupDto>, ICloneable<RaidGroup>, ICreatableFromDto<RaidGroup, RaidGroupDto>
{
    public static string DataTypeName => CommonLoc.DataTypeName_RaidGroup;

    public static HrtId.IdType IdType => HrtId.IdType.Group;

    public static RaidGroup Empty => new();

    #region Serialized

    [JsonProperty("Members")] private readonly Reference<Player>?[] _members;

    [JsonProperty("Name")] public string Name;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public RolePriority? RolePriority;

    [JsonProperty("TimeStamp")] public DateTime TimeStamp;

    [JsonProperty("Type")] public GroupType Type;

    [JsonProperty("TypeLocked")] public bool TypeLocked;

    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = [];

    #endregion

    #region Constructors

    public RaidGroup() : this("") { }

    public RaidGroup(string name) : this(name, GroupType.Raid) { }

    public RaidGroup(string name, GroupType type)
    {
        Type = type;
        TimeStamp = DateTime.Now;
        Name = name;
        _members = new Reference<Player>[8];
        for (int i = 0; i < _members.Length; i++)
        {
            _members[i] = new Player();
        }
    }

    #endregion

    #region Properties

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
            var player = _members[idx] ??= new Player();
            return player.Data;
        }
        set
        {
            if (idx < 0 || idx >= Count)
                throw new IndexOutOfRangeException($"Raid group of type {Type} has no member at index {idx}");
            if (Type == GroupType.Group)
                idx *= 2;
            _members[idx] = value;
        }
    }

    public IEnumerator<Player> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].Filled)
                yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    string IHrtDataType.Name => Name;

    IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    #endregion

    public RaidGroup Clone() => CloneService.Clone(this);

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
        (_members[idx1], _members[idx2]) = (_members[idx2], _members[idx1]);
    }
    public RaidGroupDto ToDto() => new(this);
    public void UpdateFromDto(RaidGroupDto dto) => throw new NotImplementedException();
    
    public static RaidGroup FromDto(RaidGroupDto dto) => throw new NotImplementedException();
}