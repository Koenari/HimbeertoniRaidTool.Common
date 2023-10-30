using System;
using System.Collections;
using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Security;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidGroup : IEnumerable<Player>, IHasHrtId
{
    public HrtId.IdType IdType => HrtId.IdType.Group;

    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();

    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;
    [JsonProperty("TimeStamp")] public DateTime TimeStamp;
    [JsonProperty("Name")] public string Name;
    [JsonProperty("Members")] private readonly Player[] _players;
    [JsonProperty("Type")] public GroupType Type;
    [JsonProperty("TypeLocked")] public bool TypeLocked;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public RolePriority? RolePriority = null;

    private IEnumerable<Player> Players
    {
        get
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Filled)
                    yield return this[i];
        }
    }

    public int Count => Type switch
    {
        GroupType.Solo => 1,
        GroupType.Group => 4,
        GroupType.Raid => 8,
        _ => throw new NotImplementedException(),
    };

    [JsonConstructor]
    public RaidGroup(string name = "", GroupType type = GroupType.Raid)
    {
        Type = type;
        TimeStamp = DateTime.Now;
        Name = name;
        _players = new Player[8];
        for (int i = 0; i < _players.Length; i++) _players[i] = new Player();
    }

    public Player this[int idx]
    {
        get
        {
            if (idx >= Count)
                throw new IndexOutOfRangeException($"Raidgroup of type {Type} has no member at index {idx}");
            if (Type == GroupType.Group)
                idx *= 2;
            return _players[idx];
        }
        set
        {
            if (idx >= Count)
                throw new IndexOutOfRangeException($"Raidgroup of type {Type} has no member at index {idx}");
            if (Type == GroupType.Group)
                idx *= 2;
            _players[idx] = value;
        }
    }

    public IEnumerator<Player> GetEnumerator() => Players.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Players.GetEnumerator();
}

[JsonObject(MemberSerialization.OptIn)]
public class Alliance
{
    [JsonProperty("Name")] public string Name { get; set; }
    public DateTime TimeStamp { get; set; }
    [JsonProperty("Groups")] public RaidGroup[] RaidGroups { get; set; }

    [JsonConstructor]
    public Alliance(string name = "")
    {
        Name = name;
        TimeStamp = DateTime.Now;
        RaidGroups = new RaidGroup[3];
    }
}