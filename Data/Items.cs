using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Lumina.Data;

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

    [JsonProperty] public bool IsHq { get; init; } = false;

    [JsonIgnore] public IEnumerable<Job> Jobs => Item?.ClassJobCategory.Value?.ToJob() ?? Enumerable.Empty<Job>();

    [JsonIgnore] public EquipSlotCategory EquipSlotCategory => Item?.EquipSlotCategory.Value ?? _emptyEquipSlotCategory;

    [JsonIgnore] public IEnumerable<GearSetSlot> Slots => EquipSlotCategory.AvailableSlots();

    [JsonIgnore] public bool IsUnique => Item?.IsUnique ?? true;

    [JsonProperty("Materia")] private readonly List<HrtMateria> _materia = new();

    [JsonIgnore] public IEnumerable<HrtMateria> Materia => _materia;

    [JsonIgnore] public int MateriaSlotCount => Item?.MateriaSlotCount ?? 0;

    [JsonIgnore]
    public int MaxMateriaSlots =>
        Item?.IsAdvancedMeldingPermitted ?? false ? 5 : Item?.MateriaSlotCount ?? 2;

    [JsonIgnore] public int StatCap => _statCapImpl.Value;

    [JsonIgnore] private readonly Lazy<int> _statCapImpl;

    //This holds the total stats of this gear item (including materia)
    [JsonIgnore] private readonly Dictionary<StatType, int> _statCache = new();

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

        if (!includeMateria)
            return result;
        result += _materia.Where(x => x.StatType == type).Sum(materia => materia.GetStat());
        if (type.IsSecondary())
            result = int.Min(result, StatCap);
        _statCache.TryAdd(type, result);
        return result;
    }

    public GearItem(uint id = 0) : base(id)
    {
        _statCapImpl = new Lazy<int>(() =>
        {
            //Fail in a safe way and do not cap values
            if (Item is null) return int.MaxValue;
            int maxVal = int.Max(Item.UnkData59[2].BaseParamValue, Item.UnkData59[3].BaseParamValue);
            if (IsHq)
                maxVal += int.Max(Item.UnkData73[2].BaseParamValueSpecial, Item.UnkData73[3].BaseParamValueSpecial);
            return maxVal;
        });
    }

    public bool Equals(GearItem? other) => Equals(other, ItemComparisonMode.Full);

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
            .GetExpansionByLevel(Item?.LevelEquip ?? ServiceManager.GameInfo.CurrentExpansion.MaxLevel).MaxMateriaLevel;
        if (_materia.Count >= Item?.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }

    public IEnumerable<StatType> StatTypesAffected
    {
        get
        {
            HashSet<StatType> done = new();
            if (Item is null) yield break;
            foreach (Item.ItemUnkData59Obj? stat in Item.UnkData59)
            {
                var type = (StatType)stat.BaseParam;
                done.Add(type);
                yield return type;
            }

            foreach (HrtMateria mat in Materia)
            {
                if (done.Contains(mat.StatType))
                    continue;
                done.Add(mat.StatType);
                yield return mat.StatType;
            }
        }
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtItem : IEquatable<HrtItem>
{
    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    private readonly uint _id = 0;

    public virtual uint Id => _id;

    private Item? _itemCache = null;

    [JsonIgnore] public Rarity Rarity => (Rarity)(Item?.Rarity ?? 0);

    [JsonIgnore] public ushort Icon => Item?.Icon ?? 0;

    protected Item? Item => _itemCache ??= ItemSheet?.GetRow(Id);

    public string Name => Item?.Name.RawString ?? "";

    public bool IsGear => this is GearItem || (Item?.ClassJobCategory.Row ?? 0) != 0;
    public ItemSource Source => ServiceManager.ItemInfo.GetSource(this);

    [JsonIgnore] public Lazy<uint> LevelCache;

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

    [JsonIgnore] protected static readonly ExcelSheet<Item>? ItemSheet = ServiceManager.ExcelModule.GetSheet<Item>();

    public HrtItem(uint id)
    {
        _id = id;
        LevelCache = new Lazy<uint>(() => Item?.LevelItem.Row ?? 0);
    }
    public override bool Equals(object? obj) => Equals(obj as HrtItem);
    public bool Equals(HrtItem? obj) => Id == obj?.Id;

    public override int GetHashCode() => Id.GetHashCode();
}

[JsonObject(MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class HrtMateria : HrtItem, IEquatable<HrtMateria>
{
    //Begin Object
    [JsonProperty("Category")] public readonly MateriaCategory Category;
    [JsonProperty("MateriaLevel")] private readonly byte _materiaLevel;
    [JsonIgnore] public MateriaLevel Level => (MateriaLevel)_materiaLevel;

    [JsonIgnore]
    private static readonly ExcelSheet<Materia>? _materiaSheet = ServiceManager.ExcelModule?.GetSheet<Materia>();

    [JsonIgnore] private readonly Lazy<uint> _idCache;
    [JsonIgnore] public override uint Id => _idCache.Value;
    public Materia? Materia => _materiaSheet?.GetRow((ushort)Category);
    public StatType StatType => Category.GetStatType();

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

    public int GetStat() => Materia?.Value[_materiaLevel] ?? 0;

    public bool Equals(HrtMateria? other) => base.Equals(other);
}

public class ItemIdRange : ItemIdCollection
{
    public ItemIdRange(uint start, uint end) : base(Enumerable.Range((int)start, Math.Max(0, (int)end - (int)start + 1))
        .ToList().ConvertAll(x => (uint)x))
    {
    }
}

public class ItemIdList : ItemIdCollection
{
    public static implicit operator ItemIdList(uint[] ids) => new(ids);

    public ItemIdList(params uint[] ids) : base(ids)
    {
    }

    public ItemIdList(ItemIdCollection col, params uint[] ids) : base(col.Concat(ids))
    {
    }

    public ItemIdList(IEnumerable<uint> ids) : base(ids)
    {
    }
}

public abstract class ItemIdCollection : IEnumerable<uint>
{
    public static readonly ItemIdCollection Empty = new ItemIdList();
    private readonly ReadOnlyCollection<uint> _iDs;
    public int Count => _iDs.Count;

    protected ItemIdCollection(IEnumerable<uint> ids)
    {
        _iDs = new ReadOnlyCollection<uint>(ids.ToList());
    }

    public IEnumerator<uint> GetEnumerator() => _iDs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _iDs.GetEnumerator();

    public static implicit operator ItemIdCollection(uint id) => new ItemIdList(id);

    public static implicit operator ItemIdCollection((uint, uint) id) => new ItemIdRange(id.Item1, id.Item2);
}

public enum ItemComparisonMode
{
    /// <summary>
    /// Ignores everything besides the item ID
    /// </summary>
    IdOnly,

    /// <summary>
    /// Ignores affixed materia when comparing
    /// </summary>
    IgnoreMateria,

    /// <summary>
    /// Compares all aspects of the item
    /// </summary>
    Full,
}