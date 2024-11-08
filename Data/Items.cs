﻿using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using HimbeertoniRaidTool.Common.GameData;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class GearItem : HrtItem, IEquatable<GearItem>
{
    [JsonIgnore] public static readonly GearItem Empty = new();
    [JsonIgnore] private static readonly EquipSlotCategory _emptyEquipSlotCategory = new()
    {
        RowId = 0,
        SubRowId = 0,
        SheetLanguage = Language.None,
        MainHand = 0,
        OffHand = 0,
        Head = 0,
        Body = 0,
        Gloves = 0,
        Waist = 0,
        Legs = 0,
        Feet = 0,
        Ears = 0,
        Neck = 0,
        Wrists = 0,
        FingerL = 0,
        FingerR = 0,
        SoulCrystal = 0,
    };

    [JsonProperty("Materia")] private readonly List<HrtMateria> _materia = new();

    //This holds the total stats of this gear item (including materia)
    [JsonIgnore] private readonly Dictionary<StatType, int> _statCache = new();

    [JsonIgnore] private readonly Lazy<int> _statCapImpl;

    [JsonProperty("RelicParams", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<StatType, int>? RelicStats;

    public GearItem(uint id = 0) : base(id)
    {
        _statCapImpl = new Lazy<int>(() =>
        {
            //Fail in a safe way and do not cap values
            if (Item is null) return int.MaxValue;
            //ToDo: Do actual Cap
            if (Item.Rarity == (byte)Rarity.Relic) return int.MaxValue;
            int maxVal = int.Max(Item.UnkData59[2].BaseParamValue, Item.UnkData59[3].BaseParamValue);
            if (IsHq)
                maxVal += int.Max(Item.UnkData73[2].BaseParamValueSpecial, Item.UnkData73[3].BaseParamValueSpecial);
            return maxVal;
        });
    }
    [JsonIgnore] public new string DataTypeName => CommonLoc.DataTypeName_item_gear;

    [JsonProperty] public bool IsHq { get; init; }

    [JsonIgnore] public IEnumerable<Job> Jobs => Item?.ClassJobCategory.Value?.ToJob() ?? Enumerable.Empty<Job>();

    [JsonIgnore] public EquipSlotCategory EquipSlotCategory => Item?.EquipSlotCategory.Value ?? _emptyEquipSlotCategory;

    [JsonIgnore] public IEnumerable<GearSetSlot> Slots => EquipSlotCategory.AvailableSlots();

    [JsonIgnore] public bool IsUnique => Item?.IsUnique ?? true;

    [JsonIgnore] public IEnumerable<HrtMateria> Materia => _materia;

    [JsonIgnore] public int MateriaSlotCount => Item?.MateriaSlotCount ?? 0;

    [JsonIgnore]
    public int MaxMateriaSlots =>
        Item?.IsAdvancedMeldingPermitted ?? false ? 5 : Item?.MateriaSlotCount ?? 2;

    [JsonIgnore] public int StatCap => _statCapImpl.Value;

    public IEnumerable<StatType> StatTypesAffected
    {
        get
        {
            SortedSet<StatType> done = new();
            if (Item is null) return Enumerable.Empty<StatType>();
            foreach (Item.ItemUnkData59Obj? stat in Item.UnkData59)
            {
                var type = (StatType)stat.BaseParam;
                done.Add(type);
            }
            if (IsRelic() && RelicStats is not null)
            {
                foreach ((StatType type, int value) in RelicStats)
                {
                    if (value > 0)
                        done.Add(type);
                }
            }
            foreach (HrtMateria mat in Materia)
            {
                done.Add(mat.StatType);
            }
            return done;
        }
    }
    public bool Equals(GearItem? other) => Equals(other, ItemComparisonMode.Full);

    private void InvalidateCache() => _statCache.Clear();

    public int GetStat(StatType type, bool includeMateria = true)
    {
        if (includeMateria && _statCache.TryGetValue(type, out int cached))
            return cached;
        if (Item is null) return 0;
        int result = 0;
        switch (type)
        {
            case StatType.PhysicalDamage:
                result += Item.DamagePhys;
                break;
            case StatType.MagicalDamage:
                result += Item.DamageMag;
                break;
            case StatType.Defense:
                result += Item.DefensePhys;
                break;
            case StatType.MagicDefense:
                result += Item.DefenseMag;
                break;
            case StatType.Delay:
                result += Item.Delayms;
                break;
            default:
                if (IsHq)
                    foreach (Item.ItemUnkData73Obj? param in
                             Item.UnkData73.Where(x => x.BaseParamSpecial == (byte)type))
                    {
                        result += param.BaseParamValueSpecial;
                    }

                foreach (Item.ItemUnkData59Obj? param in Item.UnkData59.Where(x => x.BaseParam == (byte)type))
                {
                    result += param.BaseParamValue;
                }
                break;
        }
        if (IsRelic() && (RelicStats?.TryGetValue(type, out int val) ?? false))
            result += val;
        if (!includeMateria)
            return result;
        result += _materia.Where(x => x.StatType == type).Sum(materia => materia.GetStat());
        if (type.IsSecondary())
            result = int.Min(result, StatCap);
        _statCache.TryAdd(type, result);
        return result;
    }

    public bool Equals(GearItem? other, ItemComparisonMode mode)
    {
        //idOnly
        if (Id != other?.Id) return false;
        if (mode == ItemComparisonMode.IdOnly) return true;
        //IgnoreMateria
        if (IsHq != other.IsHq) return false;
        if (mode == ItemComparisonMode.IgnoreMateria) return true;
        //Full
        if (_materia.Count != other._materia.Count) return false;
        Dictionary<HrtMateria, int> cnt = new();
        foreach (HrtMateria s in _materia)
        {
            if (cnt.ContainsKey(s))
                cnt[s]++;
            else
                cnt.Add(s, 1);
        }
        foreach (HrtMateria s in other._materia)
        {
            if (cnt.ContainsKey(s))
                cnt[s]--;
            else
                return false;
        }

        return cnt.Values.All(s => s == 0);
    }
    public bool IsRelic() => GameItem?.Rarity == Rarity.Relic;
    public bool CanAffixMateria() => _materia.Count < MaxMateriaSlots;

    public void AddMateria(HrtMateria materia)
    {
        if (CanAffixMateria())
        {
            _materia.Add(materia);
            InvalidateCache();
        }
    }

    public void RemoveMateria(int removeAt)
    {
        if (removeAt >= _materia.Count) return;
        _materia.RemoveAt(removeAt);
        InvalidateCache();
    }

    public void ReplaceMateria(int index, HrtMateria newMat)
    {
        if (index >= _materia.Count) return;
        _materia[index] = newMat;
        InvalidateCache();
    }

    //ToDo: Do from Lumina
    public MateriaLevel MaxAffixableMateriaLevel()
    {
        if (!CanAffixMateria()) return 0;
        MateriaLevel maxAllowed = ServiceManager.GameInfo
                                                .GetExpansionByLevel(
                                                    Item?.LevelEquip ?? ServiceManager.GameInfo.CurrentExpansion
                                                        .MaxLevel).MaxMateriaLevel;
        if (_materia.Count >= Item?.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtItem : IEquatable<HrtItem>, IHrtDataType
{

    [JsonIgnore] protected static readonly ExcelSheet<Item>? ItemSheet = ServiceManager.ExcelModule.GetSheet<Item>();
    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    private readonly uint _id;

    private Item? _itemCache;
    private LuminaItem? _luminaItemCache;

    [JsonIgnore] public Lazy<uint> LevelCache;

    public HrtItem(uint id)
    {
        _id = id;
        LevelCache = new Lazy<uint>(() => Item?.LevelItem.Row ?? 0);
    }

    public virtual uint Id => _id;

    [JsonIgnore] public Rarity Rarity => (Rarity)(Item?.Rarity ?? 0);

    [JsonIgnore] public ushort Icon => Item?.Icon ?? 0;

    protected LuminaItem? GameItem => _luminaItemCache ??= Item is null ? null : new LuminaItem(Item);

    protected Item? Item => _itemCache ??= ItemSheet?.GetRow(Id);

    public bool IsGear => this is GearItem || (Item?.ClassJobCategory.Row ?? 0) != 0;
    public ItemSource Source => ServiceManager.ItemInfo.GetSource(this);

    [JsonIgnore] public uint ItemLevel => LevelCache.Value;

    public bool Filled => Id > 0;
    public bool IsExchangableItem => ServiceManager.ItemInfo.UsedAsShopCurrency(Id);
    public bool IsContainerItem => ServiceManager.ItemInfo.IsItemContainer(Id);

    public IEnumerable<GearItem> PossiblePurchases
    {
        get
        {
            if (IsExchangableItem)
                foreach (uint canBuy in ServiceManager.ItemInfo.GetPossiblePurchases(Id))
                {
                    yield return new GearItem(canBuy);
                }
            if (IsContainerItem)
                foreach (uint id in ServiceManager.ItemInfo.GetContainerContents(Id))
                {
                    yield return new GearItem(id);
                }
        }
    }
    public bool Equals(HrtItem? obj) => Id == obj?.Id;

    public string Name => Item?.Name.RawString ?? "";
    [JsonIgnore] public string DataTypeName => CommonLoc.DataTypeName_item;
    public override string ToString() => Name;
    public override bool Equals(object? obj) => Equals(obj as HrtItem);

    public override int GetHashCode() => Id.GetHashCode();
}

[JsonObject(MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class HrtMateria : HrtItem, IEquatable<HrtMateria>
{
    [JsonIgnore]
    private static readonly ExcelSheet<Materia>? _materiaSheet = ServiceManager.ExcelModule.GetSheet<Materia>();

    [JsonIgnore] private static Lazy<Dictionary<uint, (MateriaCategory, MateriaLevel)>> _idLookupImpl = new(() =>
    {
        var result = new Dictionary<uint, (MateriaCategory, MateriaLevel)>();
        if (_materiaSheet is null) return result;
        foreach (Materia materia in _materiaSheet)
        {
            int level = 0;
            foreach (var tier in materia.Item)
            {
                if (tier.Row == 0) continue;
                result.Add(tier.Row, ((MateriaCategory)materia.RowId, (MateriaLevel)level));
                level++;
            }
        }
        return result;
    });
    [JsonIgnore] private static Dictionary<uint, (MateriaCategory, MateriaLevel)> IdLookup => _idLookupImpl.Value;

    [JsonIgnore] private readonly Lazy<uint> _idCache;
    [JsonProperty("MateriaLevel")] private readonly byte _materiaLevel;
    //Begin Object
    [JsonProperty("Category")] public readonly MateriaCategory Category;

    public HrtMateria(uint id) : this(IdLookup[id]) { }
    public HrtMateria() : this(0, 0)
    {
    }

    public HrtMateria((MateriaCategory cat, MateriaLevel lvl) mat) : this(mat.cat, mat.lvl)
    {
    }

    [JsonConstructor]
    public HrtMateria(MateriaCategory cat, MateriaLevel lvl) : base(0)
    {
        Category = cat;
        _materiaLevel = (byte)lvl;
        _idCache = new Lazy<uint>(() => Materia?.Item[_materiaLevel].Row ?? 0);
    }
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_materia;
    [JsonIgnore] public new string DataTypeName => DataTypeNameStatic;
    [JsonIgnore] public MateriaLevel Level => (MateriaLevel)_materiaLevel;
    [JsonIgnore] public override uint Id => _idCache.Value;
    public Materia? Materia => _materiaSheet?.GetRow((ushort)Category);
    public StatType StatType => Category.GetStatType();

    public bool Equals(HrtMateria? other) => base.Equals(other);

    public int GetStat() => Materia?.Value[_materiaLevel] ?? 0;
}

public class ItemIdRange : ItemIdCollection
{
    public ItemIdRange(uint start, uint end) : base(Enumerable.Range((int)start, Math.Max(0, (int)end - (int)start + 1))
                                                              .ToList().ConvertAll(x => (uint)x))
    {
    }
    public ItemIdRange(int start, int end) : base(Enumerable.Range(start, Math.Max(0, end - start + 1))
                                                            .ToList().ConvertAll(x => (uint)x))
    {
    }
}

public class ItemIdList : ItemIdCollection
{

    public ItemIdList(params uint[] ids) : base(ids)
    {
    }

    public ItemIdList(ItemIdCollection col, params uint[] ids) : base(col.Concat(ids))
    {
    }

    public ItemIdList(IEnumerable<uint> ids) : base(ids)
    {
    }
    public static implicit operator ItemIdList(uint[] ids) => new(ids);
}

public abstract class ItemIdCollection : IEnumerable<uint>
{
    public static readonly ItemIdCollection Empty = new ItemIdList();
    private readonly ReadOnlyCollection<uint> _iDs;

    protected ItemIdCollection(IEnumerable<uint> ids)
    {
        _iDs = new ReadOnlyCollection<uint>(ids.ToList());
    }
    public int Count => _iDs.Count;

    public IEnumerator<uint> GetEnumerator() => _iDs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _iDs.GetEnumerator();

    public static implicit operator ItemIdCollection(uint id) => new ItemIdList(id);

    public static implicit operator ItemIdCollection(Range range) =>
        new ItemIdRange(range.Start.Value, range.End.Value);

    public static implicit operator ItemIdCollection((uint, uint) id) => new ItemIdRange(id.Item1, id.Item2);
}

public enum ItemComparisonMode
{
    /// <summary>
    ///     Ignores everything besides the item ID
    /// </summary>
    IdOnly,

    /// <summary>
    ///     Ignores affixed materia when comparing
    /// </summary>
    IgnoreMateria,

    /// <summary>
    ///     Compares all aspects of the item
    /// </summary>
    Full,
}