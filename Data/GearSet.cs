using System.Collections;
using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;
using Lumina.Excel.Sheets;
using XIVCalc.Calculations;
using XIVCalc.Interfaces;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReadOnlyGearSet
{
    GearItem this[GearSetSlot slot] { get; }

    FoodItem? Food { get; }
    int GetStat(StatType type);
    public GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None);
}

[JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore)]
public class GearSet : IEnumerable<GearItem>, IReadOnlyGearSet, IHrtDataTypeWithId
{
    public const int NUM_SLOTS = 12;
    //Actual Gear data
    [JsonProperty("Items")] private readonly GearItem?[] _items = new GearItem?[NUM_SLOTS];

    //Caches
    [JsonIgnore] private int? _levelCache;

    //Properties
    [JsonProperty("LastEtroFetched")] [Obsolete("Use LastExternalFetchDate", true)]
    public DateTime EtroFetchDate { set => LastExternalFetchDate = value; }

    [JsonProperty("EtroID")] [Obsolete("Use ExternalId", true)]
    public string EtroId { set => ExternalId = value; }

    [JsonProperty("Food")] public FoodItem? Food { get; set; }

    [JsonProperty("ExternalId")] public string ExternalId = "";
    [JsonProperty("ExternalIdx")] public int ExternalIdx = 0;
    [JsonProperty("LastExternalFetch")] public DateTime LastExternalFetchDate;

    [JsonProperty("ManagedBy")] public GearSetManager ManagedBy;
    [JsonProperty("Name")] public string Name = "";
    [JsonProperty("Alias")] public string? Alias;

    [JsonProperty("RemoteIDs")] public List<HrtId> RemoteIDs = new();


    [JsonProperty("TimeStamp")] public DateTime TimeStamp;

    public bool IsManagedExternally => ManagedBy != GearSetManager.Hrt;

    [JsonConstructor]
    public GearSet() : this(GearSetManager.Hrt) { }
    public GearSet(GearSetManager manager, string name = "")
    {
        ManagedBy = manager;
        Name = name;
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            this[i] = new GearItem();
        }
        IsSystemManaged = manager == GearSetManager.Etro;
    }

    public GearSet(GearSet copyFrom)
    {
        CopyFrom(copyFrom);
    }
    [JsonProperty("IsManaged")] public bool IsSystemManaged { get; private set; }

    //Runtime only properties
    public bool IsEmpty => this.All(x => x is { Id: 0 });

    public int ItemLevel => _levelCache ??= CalcItemLevel();

    private GearItem this[int idx]
    {
        get => _items[idx] ??= new GearItem();
        set
        {
            _items[idx] = value;
            InvalidateCaches();
        }
    }

    public GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None) =>
        new(job, this, tribe, bonus);

    public static IEnumerable<GearSetSlot> Slots => Enum.GetValues<GearSetSlot>()
                                                        .Where(slot => slot < GearSetSlot.SoulCrystal
                                                                    && slot != GearSetSlot.Waist);
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_GearSet;
    public IEnumerator<GearItem> GetEnumerator() => AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    string IHrtDataType.Name => Alias ?? Name;

    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Gear;
    [JsonIgnore] public string DataTypeName => DataTypeNameStatic;
    //IDs
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;
    [JsonIgnore] IList<HrtId> IHasHrtId.RemoteIds => RemoteIDs;
    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);

    public GearItem this[GearSetSlot slot]
    {
        get => this[ToIndex(slot)];
        set => this[ToIndex(slot)] = value;
    }

    /*
     * Caching stats is a problem since this needs to be invalidated when changing materia
     */
    public int GetStat(StatType type) => this.Sum(x => x.GetStat(type));

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

    private static int ToIndex(GearSetSlot slot) => slot switch
    {
        GearSetSlot.MainHand => 0,
        GearSetSlot.OffHand  => 11,
        GearSetSlot.Head     => 1,
        GearSetSlot.Body     => 2,
        GearSetSlot.Hands    => 3,
        GearSetSlot.Legs     => 4,
        GearSetSlot.Feet     => 5,
        GearSetSlot.Ear      => 6,
        GearSetSlot.Neck     => 7,
        GearSetSlot.Wrist    => 8,
        GearSetSlot.Ring1    => 9,
        GearSetSlot.Ring2    => 10,
        _                    => throw new IndexOutOfRangeException($"GearSlot {slot} does not exist"),
    };

    public void CopyFrom(GearSet gearSet)
    {
        TimeStamp = gearSet.TimeStamp;
        ExternalId = gearSet.ExternalId;
        ExternalIdx = gearSet.ExternalIdx;
        LastExternalFetchDate = gearSet.LastExternalFetchDate;
        Name = gearSet.Name;
        Alias = gearSet.Alias;
        ManagedBy = gearSet.ManagedBy;
        RemoteIDs = gearSet.RemoteIDs;
        Food = gearSet.Food;
        //Do an actual copy of the item
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = gearSet._items[i]?.Clone();
        }
        InvalidateCaches();
    }

    private IEnumerable<GearItem> AsEnumerable()
    {
        for (int i = 0; i < NUM_SLOTS; ++i)
        {
            yield return this[i];
        }
    }
    public override string ToString() => $"{Alias ?? Name} ({ItemLevel})";
}

internal class GearSetOverride : IReadOnlyGearSet
{
    private readonly IReadOnlyGearSet _base;
    private readonly GearItem _override;
    private readonly GearSetSlot _slot;

    public GearSetOverride(IReadOnlyGearSet baseSet, GearSetSlot slot, GearItem item)
    {
        _base = baseSet;
        _slot = slot;
        _override = item;
    }

    public GearItem this[GearSetSlot slot] => slot == _slot ? _override : _base[slot];

    public FoodItem? Food => _base.Food;
    public int GetStat(StatType statType)
    {
        int result = _base.GetStat(statType);
        result -= _base[_slot].GetStat(statType);
        result += _override.GetStat(statType);
        return result;
    }
    public GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None) =>
        new(job, this, tribe, bonus);
}

public static class GearSetExtensions
{
    public static IReadOnlyGearSet With(this IReadOnlyGearSet baseSet, GearItem item,
                                        GearSetSlot slot = GearSetSlot.None)
    {
        if (slot is GearSetSlot.None)
            slot = item.Slots.FirstOrDefault(GearSetSlot.None);
        return new GearSetOverride(baseSet, slot, item);
    }
}