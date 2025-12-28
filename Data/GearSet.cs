using System.Collections;
using HimbeertoniRaidTool.Common.Data.Dto;
using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn, MissingMemberHandling = MissingMemberHandling.Ignore)]
public class GearSet : IEnumerable<GearItem>, IReadOnlyGearSet, IHrtDataTypeWithDto<GearSet, GearSetDto>,
                       ICloneable<GearSet>
{
    public const int NUM_SLOTS = 12;

    public static string DataTypeName => CommonLoc.DataTypeName_GearSet;
    public static HrtId.IdType IdType => HrtId.IdType.Gear;

    public static GearSet Empty => new();

    #region Serialized

    //Header data
    [JsonProperty("ManagedBy")] public GearSetManager ManagedBy;

    [JsonProperty("Name")] public string Name = "";

    [JsonProperty("Alias")] public string? Alias;

    [JsonProperty("TimeStamp")] public DateTime TimeStamp;

    [JsonProperty("IsManaged")] public bool IsSystemManaged { get; private set; }

    //Actual Gear data
    [JsonProperty("Items")] private readonly GearItem?[] _items = new GearItem?[NUM_SLOTS];

    [JsonProperty("Food")] public FoodItem? Food { get; set; }

    //HRT/XRT Ids
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    [JsonProperty("RemoteIDs")] public List<HrtId> RemoteIDs = [];

    /*
     * External Service data (non HRT/XRT)
     */
    [JsonProperty("ExternalId")] public string ExternalId = "";
    [JsonProperty("ExternalIdx")] public int ExternalIdx;
    [JsonProperty("LastExternalFetch")] public DateTime LastExternalFetchDate;

    #endregion

    #region Constructors

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

    #endregion

    #region Properties

    string IHrtDataType.Name => Alias ?? Name;

    public bool IsManagedExternally => ManagedBy != GearSetManager.Hrt;

    public bool IsEmpty => this.All(x => x is { Id: 0 });

    public int ItemLevel => _levelCache ??= CalcItemLevel();

    IList<HrtId> IHasHrtId.RemoteIds => RemoteIDs;

    public GearItem this[GearSetSlot slot]
    {
        get => this[ToIndex(slot)];
        set => this[ToIndex(slot)] = value;
    }

    private int? _levelCache;

    private GearItem this[int idx]
    {
        get => _items[idx] ??= new GearItem();
        set
        {
            _items[idx] = value;
            InvalidateCaches();
        }
    }

    #endregion

    public GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None) =>
        new(job, this, tribe, bonus);

    public static IEnumerable<GearSetSlot> Slots => Enum.GetValues<GearSetSlot>()
                                                        .Where(slot => slot < GearSetSlot.SoulCrystal
                                                                    && slot != GearSetSlot.Waist);

    public GearSet Clone() => CloneService.Clone(this);

    public IEnumerator<GearItem> GetEnumerator() => AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);

    // Caching stats is a problem since this needs to be invalidated when changing materia
    public int GetStat(StatType type) => this.Sum(x => x.GetStat(type));

    public void MarkAsSystemManaged() => IsSystemManaged = true;

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

    public override string ToString() => $"{Alias ?? Name} ({ItemLevel})";

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
        GearSetSlot.Waist or GearSetSlot.SoulCrystal or GearSetSlot.None => throw new IndexOutOfRangeException(
            $"GearSlot {slot} does not exist"),
        _ => throw new IndexOutOfRangeException($"GearSlot {slot} does not exist"),
    };

    private IEnumerable<GearItem> AsEnumerable()
    {
        for (int i = 0; i < NUM_SLOTS; ++i)
        {
            yield return this[i];
        }
    }
    public GearSetDto ToDto() => new(this);
    public void UpdateFromDto(GearSetDto dto) => throw new NotImplementedException();
    
    public static GearSet FromDto(GearSetDto dto) => throw  new NotImplementedException();
}

internal class GearSetOverride(IReadOnlyGearSet baseSet, GearSetSlot slot, GearItem item) : IReadOnlyGearSet
{
    public GearItem this[GearSetSlot slot1] => slot1 == slot ? item : baseSet[slot1];

    public FoodItem? Food => baseSet.Food;
    public int GetStat(StatType statType)
    {
        int result = baseSet.GetStat(statType);
        result -= baseSet[slot].GetStat(statType);
        result += item.GetStat(statType);
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