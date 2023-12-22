using HimbeertoniRaidTool.Common.Security;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReadOnlyGearSet
{
    GearItem this[GearSetSlot slot] { get; }
    int GetStat(StatType type);
}

[JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore)]
public class GearSet : IEnumerable<GearItem>, IReadOnlyGearSet, IHasHrtId
{
    public const int NUM_SLOTS = 12;

    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Gear;
    //IDs
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    [JsonProperty("RemoteIDs")] public List<HrtId> RemoteIDs = new();
    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIDs;

    [JsonProperty("EtroID")] public string EtroId = "";

    //Properties
    [JsonProperty("TimeStamp")] public DateTime TimeStamp;
    [JsonProperty("LastEtroFetched")] public DateTime EtroFetchDate;
    [JsonProperty("Name")] public string Name = "";

    [JsonProperty("ManagedBy")] public GearSetManager ManagedBy;
    [JsonProperty("IsManaged")] public bool IsSystemManaged { get; private set; } = false;
    //Actual Gear data
    [JsonProperty("Items")] private readonly GearItem?[] _items = new GearItem?[NUM_SLOTS];

    //Runtime only properties
    public bool IsEmpty => this.All(x => x is { Id: 0 });

    public int ItemLevel => _levelCache ??= CalcItemLevel();

    //Caches
    [JsonIgnore] private int? _levelCache = null;

    [JsonConstructor]
    public GearSet(GearSetManager manager = GearSetManager.Hrt, string name = "")
    {
        ManagedBy = manager;
        Name = name;
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            this[i] = new GearItem(0);
        }
        IsSystemManaged = manager == GearSetManager.Etro;
    }

    public GearSet(GearSet copyFrom)
    {
        CopyFrom(copyFrom);
    }

    public GearItem this[GearSetSlot slot]
    {
        get => this[ToIndex(slot)];
        set => this[ToIndex(slot)] = value;
    }

    private GearItem this[int idx]
    {
        get => _items[idx] ??= new GearItem();
        set
        {
            _items[idx] = value;
            InvalidateCaches();
        }
    }

    public void MarkAsSystemManaged() => IsSystemManaged = true;

    private void InvalidateCaches() => _levelCache = null;

    private int CalcItemLevel()
    {
        uint itemLevel = 0;
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            itemLevel += this[i].ItemLevel;
            if (this[i].EquipSlotCategory.Disallows(GearSetSlot.OffHand))
                itemLevel += this[i].ItemLevel;
        }

        return (int)((float)itemLevel / NUM_SLOTS);
    }

    /*
     * Caching stats is a problem since this needs to be invalidated when changing materia
     */
    public int GetStat(StatType type) => this.Sum(x => x.GetStat(type));

    private static int ToIndex(GearSetSlot slot) => slot switch
    {
        GearSetSlot.MainHand => 0,
        GearSetSlot.OffHand => 11,
        GearSetSlot.Head => 1,
        GearSetSlot.Body => 2,
        GearSetSlot.Hands => 3,
        GearSetSlot.Legs => 4,
        GearSetSlot.Feet => 5,
        GearSetSlot.Ear => 6,
        GearSetSlot.Neck => 7,
        GearSetSlot.Wrist => 8,
        GearSetSlot.Ring1 => 9,
        GearSetSlot.Ring2 => 10,
        _ => throw new IndexOutOfRangeException($"GearSlot {slot} does not exist"),
    };

    public void CopyFrom(GearSet gearSet)
    {
        TimeStamp = gearSet.TimeStamp;
        EtroId = gearSet.EtroId;
        Name = gearSet.Name;
        ManagedBy = gearSet.ManagedBy;
        //Do an actual copy of the item
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = gearSet._items[i].Clone();
        }
        InvalidateCaches();
    }

    public static IEnumerable<GearSetSlot> Slots => Enum.GetValues<GearSetSlot>()
        .Where(slot => slot < GearSetSlot.SoulCrystal && slot != GearSetSlot.Waist);

    private IEnumerable<GearItem> AsEnumerable()
    {
        for (int i = 0; i < NUM_SLOTS; ++i)
        {
            yield return this[i];
        }
    }
    public IEnumerator<GearItem> GetEnumerator() => AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);
}

internal class GearSetOverride : IReadOnlyGearSet
{
    private readonly IReadOnlyGearSet _base;
    private readonly GearSetSlot _slot;
    private readonly GearItem _override;

    public GearSetOverride(IReadOnlyGearSet baseSet, GearSetSlot slot, GearItem item)
    {
        _base = baseSet;
        _slot = slot;
        _override = item;
    }

    public GearItem this[GearSetSlot slot] => slot == _slot ? _override : _base[slot];

    public int GetStat(StatType statType)
    {
        int result = _base.GetStat(statType);
        result -= _base[_slot].GetStat(statType);
        result += _override.GetStat(statType);
        return result;
    }
}

public static class GearSetExtensions
{
    public static IReadOnlyGearSet With(this IReadOnlyGearSet baseSet, GearItem item,
        GearSetSlot slot = GearSetSlot.None)
    {
        if (slot is GearSetSlot.None)
            slot = item.Slots.First();
        return new GearSetOverride(baseSet, slot, item);
    }
}