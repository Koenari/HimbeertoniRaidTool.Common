using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class GearItem : HrtItem, IEquatable<GearItem>
{
    [JsonIgnore]
    public static readonly GearItem Empty = new();
    [JsonProperty]
    public bool IsHq = false;
    [JsonIgnore]
    public List<Job> Jobs => Item?.ClassJobCategory.Value?.ToJob() ?? new List<Job>();
    [JsonIgnore]
    public IEnumerable<GearSetSlot> Slots => (Item?.EquipSlotCategory.Value).AvailableSlots();
    [JsonProperty("Materia")]
    private readonly List<HrtMateria> _materia = new();
    [JsonIgnore]
    public IEnumerable<HrtMateria> Materia => _materia;
    [JsonIgnore]
    public int MaxMateriaSlots =>
        (Item?.IsAdvancedMeldingPermitted ?? false) ? 5 : (Item?.MateriaSlotCount ?? 2);
    public int GetStat(StatType type, bool includeMateria = true)
    {
        if (Item is null) return 0;
        int result = 0;
        bool isSecondary = false;
        switch (type)
        {
            case StatType.PhysicalDamage: result += Item.DamagePhys; break;
            case StatType.MagicalDamage: result += Item.DamageMag; break;
            case StatType.Defense: result += Item.DefensePhys; break;
            case StatType.MagicDefense: result += Item.DefenseMag; break;
            default:
                isSecondary = true;
                if (IsHq)
                    foreach (var param in Item.UnkData73.Where(x => x.BaseParamSpecial == (byte)type))
                        result += param.BaseParamValueSpecial;

                foreach (var param in Item.UnkData59.Where(x => x.BaseParam == (byte)type))
                    result += param.BaseParamValue;
                break;
        }
        if (includeMateria)
            foreach (var materia in _materia.Where(x => x.StatType == type))
                result += materia.GetStat();
        if (isSecondary)
        {
            int maxVal = IsHq ? Item.UnkData73[2].BaseParamValueSpecial : Item.UnkData59[2].BaseParamValue;
            if (result > maxVal)
                result = maxVal;
        }
        return result;
    }
    public GearItem(uint ID = 0) : base(ID) { }
    public bool Equals(GearItem? other) => Equals(other, ItemComparisonMode.Full);
    public bool Equals(GearItem? other, ItemComparisonMode mode)
    {
        //idOnly
        if (ID != other?.ID) return false;
        if (mode == ItemComparisonMode.IdOnly) return true;
        //IgnoreMateria
        if (IsHq != other.IsHq) return false;
        if (mode == ItemComparisonMode.IgnoreMateria) return true;
        //Full
        if (_materia.Count != other._materia.Count) return false;
        Dictionary<HrtMateria, int> cnt = new();
        foreach (var s in _materia)
        {
            if (cnt.ContainsKey(s))
                cnt[s]++;
            else
                cnt.Add(s, 1);
        }
        foreach (var s in other._materia)
        {
            if (cnt.ContainsKey(s))
                cnt[s]--;
            else
                return false;
        }
        foreach (int s in cnt.Values)
            if (s != 0)
                return false;
        return true;
    }
    public bool CanAffixMateria() => _materia.Count < MaxMateriaSlots;

    public void AddMateria(HrtMateria materia)
    {
        if (CanAffixMateria())
            _materia.Add(materia);
    }
    public void RemoveMateria(int removeAt)
    {
        if (removeAt < _materia.Count)
            _materia.RemoveAt(removeAt);
    }
    public void ReplacecMateria(int index, HrtMateria newMat)
    {
        if (index < _materia.Count)
            _materia[index] = newMat;
    }
    //ToDo: Do from Lumina
    public MateriaLevel MaxAffixableMateriaLevel()
    {
        if (!CanAffixMateria()) return 0;
        MateriaLevel maxAllowed = MateriaLevel.X;
        if (_materia.Count >= Item?.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }
}
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtItem : IEquatable<HrtItem>
{
    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    protected readonly uint _ID = 0;
    public virtual uint ID => _ID;
    private Item? ItemCache = null;
    public Item? Item => ItemCache ??= _itemSheet?.GetRow(ID);
    public string Name => Item?.Name.RawString ?? "";
    public bool IsGear => this is GearItem || (Item?.ClassJobCategory.Row ?? 0) != 0;
    public ItemSource Source => ServiceManager.ItemInfo.GetSource(this);
    [JsonIgnore]
    public uint? ILevelCache = null;
    [JsonIgnore]
    public uint ItemLevel => ILevelCache ??= Item?.LevelItem.Row ?? 0;
    public bool Filled => ID > 0;
    /*
    public string SourceShortName
    {
        get
        {
            if (Source == ItemSource.Loot && (ServiceManager.ItemInfo?.CanBeLooted(ID) ?? false))
                return ServiceManager.ItemInfo.GetLootSources(ID).First().InstanceType.FriendlyName();
            return Source.FriendlyName();
        }
    }
    */
    public bool IsExchangableItem => ServiceManager.ItemInfo?.UsedAsShopCurrency(ID) ?? false;
    public bool IsContainerItem => ServiceManager.ItemInfo?.IsItemContainer(ID) ?? false;
    public IEnumerable<GearItem> PossiblePurchases
    {
        get
        {
            if (IsExchangableItem)
                foreach (uint canBuy in ServiceManager.ItemInfo.GetPossiblePurchases(ID))
                    yield return new GearItem(canBuy);
            if (IsContainerItem)
                foreach (uint id in ServiceManager.ItemInfo.GetContainerContents(ID))
                    yield return new GearItem(id);
        }
    }
    [JsonIgnore]
    protected static readonly ExcelSheet<Item>? _itemSheet = ServiceManager.ExcelModule?.GetSheet<Item>();

    public HrtItem(uint ID) => _ID = ID;

    public bool Equals(HrtItem? obj)
    {
        return ID == obj?.ID;
    }
    public override int GetHashCode() => ID.GetHashCode();
}
[JsonObject(MemberSerialization.OptIn)]
public class HrtMateria : HrtItem, IEquatable<HrtMateria>
{
    //Begin Object
    [JsonProperty("Category")]
    public readonly MateriaCategory Category;
    [JsonProperty("MateriaLevel")]
    private readonly byte MateriaLevel;
    [JsonIgnore]
    public MateriaLevel Level => (MateriaLevel)MateriaLevel;
    [JsonIgnore]
    private static readonly ExcelSheet<Materia>? _materiaSheet = ServiceManager.ExcelModule?.GetSheet<Materia>();
    private uint? IDCache = null;
    [JsonIgnore]
    public override uint ID => IDCache ??= Materia?.Item[MateriaLevel].Row ?? 0;
    public Materia? Materia => _materiaSheet?.GetRow((ushort)Category);
    public StatType StatType => Category.GetStatType();
    public HrtMateria() : this(0, (byte)0) { }
    public HrtMateria((MateriaCategory cat, byte lvl) mat) : this(mat.cat, mat.lvl) { }
    public HrtMateria(MateriaCategory cat, MateriaLevel lvl) : base(0) => (Category, MateriaLevel) = (cat, (byte)lvl);
    [JsonConstructor]
    public HrtMateria(MateriaCategory cat, byte lvl) : base(0) => (Category, MateriaLevel) = (cat, lvl);
    public int GetStat() => Materia?.Value[MateriaLevel] ?? 0;
    public bool Equals(HrtMateria? other) => base.Equals(other);
}
public class ItemIDRange : ItemIDCollection
{
    public ItemIDRange(uint start, uint end) : base(Enumerable.Range((int)start, Math.Max(0, (int)end - (int)start + 1)).ToList().ConvertAll(x => (uint)x)) { }
}
public class ItemIDList : ItemIDCollection
{
    public static implicit operator ItemIDList(uint[] ids) => new(ids);

    public ItemIDList(params uint[] ids) : base(ids) { }
    public ItemIDList(ItemIDCollection col, params uint[] ids) : base(col.Concat(ids)) { }
    public ItemIDList(IEnumerable<uint> ids) : base(ids) { }
}
public abstract class ItemIDCollection : IEnumerable<uint>
{
    public static ItemIDCollection Empty = new ItemIDList();
    private readonly ReadOnlyCollection<uint> _IDs;
    public int Count => _IDs.Count;
    protected ItemIDCollection(IEnumerable<uint> ids) => _IDs = new(ids.ToList());
    public IEnumerator<uint> GetEnumerator() => _IDs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _IDs.GetEnumerator();
    public static implicit operator ItemIDCollection(uint id) => new ItemIDList(id);
    public static implicit operator ItemIDCollection((uint, uint) id) => new ItemIDRange(id.Item1, id.Item2);
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
    Full
}
