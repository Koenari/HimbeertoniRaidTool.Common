﻿using HimbeertoniRaidTool.Common.Security;
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

    //Actual Gear data
    [JsonProperty("Items")] private readonly GearItem[] _items = new GearItem[NUM_SLOTS];

    //Runtime only properties
    public bool IsEmpty => Array.TrueForAll(_items, x => x.Id == 0);

    public int ItemLevel => _levelCache ??= CalcItemLevel();

    //Caches
    [JsonIgnore] private int? _levelCache = null;

    [JsonConstructor]
    public GearSet(GearSetManager manager = GearSetManager.Hrt, string name = "")
    {
        ManagedBy = manager;
        Name = name;
        _items.Initialize();
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
        get => _items[idx];
        set
        {
            _items[idx] = value;
            InvalidateCaches();
        }
    }

    private void InvalidateCaches()
    {
        _levelCache = null;
    }

    private int CalcItemLevel()
    {
        uint itemLevel = 0;
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            itemLevel += _items[i].ItemLevel;
            if (_items[i].EquipSlotCategory.Disallows(GearSetSlot.OffHand))
                itemLevel += _items[i].ItemLevel;
        }

        return (int)((float)itemLevel / NUM_SLOTS);
    }

    public int Count(HrtItem item)
    {
        return Array.FindAll(_items, x => x.Equals(item)).Length;
    }

    public bool Contains(HrtItem item)
    {
        return Array.Exists(_items, x => x.Equals(item));
    }

    public bool ContainsExact(GearItem item)
    {
        return Array.Exists(_items, x => x.Equals(item, ItemComparisonMode.Full));
    }

    /*
     * Caching stats is a problem since this needs to be invalidated when changing materia
     */
    public int GetStat(StatType type)
    {
        int result = 0;
        Array.ForEach(_items, x => result += x.GetStat(type));
        return result;
    }

    private static int ToIndex(GearSetSlot slot)
    {
        return slot switch
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
    }

    public void CopyFrom(GearSet gearSet)
    {
        TimeStamp = gearSet.TimeStamp;
        EtroId = gearSet.EtroId;
        Name = gearSet.Name;
        ManagedBy = gearSet.ManagedBy;
        //Do an actual copy of the item
        for (int i = 0; i < _items.Length; i++)
            _items[i] = gearSet._items[i].Clone();
        InvalidateCaches();
    }

    public static IEnumerable<GearSetSlot> Slots
    {
        get
        {
            return Enum.GetValues<GearSetSlot>()
                .Where(slot => slot < GearSetSlot.SoulCrystal && slot != GearSetSlot.Waist);
        }
    }

    public IEnumerator<GearItem> GetEnumerator() => ((IEnumerable<GearItem>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
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