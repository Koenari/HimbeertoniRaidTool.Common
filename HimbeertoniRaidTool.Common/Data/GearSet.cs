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
public class GearSet : IEnumerable<GearItem>, IReadOnlyGearSet
{
    private const int NumSlots = 12;
    //IDs
    [JsonProperty("HrtID"), Obsolete] public string OldHrtID = "";
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtID LocalID = HrtID.Empty;
    [JsonProperty("RemoteIDs")] public List<HrtID> RemoteIDs = new();
    [JsonProperty("EtroID")] public string EtroID = "";
    //Properties
    [JsonProperty("TimeStamp")] public DateTime? TimeStamp;
    [JsonProperty("LastEtroFetched")] public DateTime EtroFetchDate;
    [JsonProperty("Name")] public string Name = "";
    [JsonProperty("ManagedBy")] public GearSetManager ManagedBy;
    //Actual Gear data
    [JsonProperty("Items")] private readonly GearItem[] Items = new GearItem[NumSlots];
    //Abstractions for Deserialization of Versions older than 1.2.0
    [JsonProperty, Obsolete] private GearItem MainHand { set => this[0] = value; }
    [JsonProperty, Obsolete] private GearItem Head { set => this[1] = value; }
    [JsonProperty, Obsolete] private GearItem Body { set => this[2] = value; }
    [JsonProperty, Obsolete] private GearItem Hands { set => this[3] = value; }
    [JsonProperty, Obsolete] private GearItem Legs { set => this[4] = value; }
    [JsonProperty, Obsolete] private GearItem Feet { set => this[5] = value; }
    [JsonProperty, Obsolete] private GearItem Ear { set => this[6] = value; }
    [JsonProperty, Obsolete] private GearItem Neck { set => this[7] = value; }
    [JsonProperty, Obsolete] private GearItem Wrist { set => this[8] = value; }
    [JsonProperty, Obsolete] private GearItem Ring1 { set => this[9] = value; }
    [JsonProperty, Obsolete] private GearItem Ring2 { set => this[10] = value; }
    [JsonProperty, Obsolete] private GearItem OffHand { set => this[11] = value; }

    //Runtime only properties
    public bool IsEmpty => Array.TrueForAll(Items, x => x.ID == 0);
    public int ItemLevel => ILevelCache ??= CalcItemLevel();
    //Caches
    [JsonIgnore] private int? ILevelCache = null;

    public GearSet()
    {
        ManagedBy = GearSetManager.HRT;
        Clear();
    }
    public GearSet(GearSetManager manager, string name = "HrtCurrent")
    {
        ManagedBy = manager;
        Name = name;
        Clear();
    }
    public void Clear()
    {
        for (int i = 0; i < NumSlots; i++)
        {
            this[i] = new(0);
        }
        InvalidateCaches();
    }
    public GearItem this[GearSetSlot slot]
    {
        get => this[ToIndex(slot)];
        set => this[ToIndex(slot)] = value;
    }
    private GearItem this[int idx]
    {
        get => Items[idx];
        set
        {
            Items[idx] = value;
            InvalidateCaches();
        }
    }
    private void InvalidateCaches()
    {
        ILevelCache = null;
    }
    private int CalcItemLevel()
    {
        uint itemLevel = 0;
        for (int i = 0; i < NumSlots; i++)
        {
            if (Items[i] != null && Items[i].ItemLevel > 0)
            {
                itemLevel += Items[i].ItemLevel;
                if (Items[i].EquipSlotCategory.Disallows(GearSetSlot.OffHand))
                    itemLevel += Items[i].ItemLevel;
            }
        }
        return (int)((float)itemLevel / NumSlots);
    }
    public bool Contains(HrtItem item) => Array.Exists(Items, x => x.Equals(item));
    public bool ContainsExact(GearItem item) => Array.Exists(Items, x => x.Equals(item, ItemComparisonMode.Full));
    /*
     * Caching stats is a problem since this needs to be invalidated when changing materia
     */
    public int GetStat(StatType type)
    {
        int result = 0;
        Array.ForEach(Items, x => result += x.GetStat(type));
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
            _ => throw new IndexOutOfRangeException("GearSlot" + slot.ToString() + "does not exist"),
        };
    }

    public void CopyFrom(GearSet gearSet)
    {
        TimeStamp = gearSet.TimeStamp;
        EtroID = gearSet.EtroID;
        Name = gearSet.Name;
        ManagedBy = gearSet.ManagedBy;
        //Do an actual copy of the item
        for (int i = 0; i < Items.Length; i++)
            Items[i] = gearSet.Items[i].Clone();
        InvalidateCaches();
    }
    public static IEnumerable<GearSetSlot> Slots
    {
        get
        {
            foreach (var slot in Enum.GetValues<GearSetSlot>())
            {
                if (slot < GearSetSlot.SoulCrystal && slot != GearSetSlot.Waist)
                    yield return slot;
            }
        }
    }
    public IEnumerator<GearItem> GetEnumerator()
    {
        return ((IEnumerable<GearItem>)Items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Items.GetEnumerator();
    }
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

    public GearItem this[GearSetSlot slot]
    {
        get
        {
            if (slot == _slot)
                return _override;
            return _base[slot];
        }
    }

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
    public static IReadOnlyGearSet With(this IReadOnlyGearSet baseSet, GearItem item, GearSetSlot slot = GearSetSlot.None)
    {
        if (slot is GearSetSlot.None)
            slot = item.Slots.First();
        return new GearSetOverride(baseSet, slot, item);
    }

}
