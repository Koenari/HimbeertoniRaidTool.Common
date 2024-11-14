using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HimbeertoniRaidTool.Common.GameData;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class GearItem : HrtItem, IEquatable<GearItem>
{
    [JsonIgnore] public static readonly GearItem Empty = new();

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
            //ToDo: Do actual Cap
            if (Item.Rarity == (byte)Rarity.Relic) return int.MaxValue;
            return int.MaxValue;
            //Todo: Figure out how to do in new Lumina
            /*
            int maxVal = int.Max(Item.UnkData59[2].BaseParamValue, Item.UnkData59[3].BaseParamValue);
            if (IsHq)
                maxVal += int.Max(Item.UnkData73[2].BaseParamValueSpecial, Item.UnkData73[3].BaseParamValueSpecial);
            return maxVal;
            */
        });
    }
    [JsonIgnore] public new string DataTypeName => CommonLoc.DataTypeName_item_gear;

    [JsonProperty] public bool IsHq { get; init; }

    [JsonIgnore] public IEnumerable<Job> Jobs => Item.ClassJobCategory.Value.ToJob();

    [JsonIgnore] public EquipSlotCategory EquipSlotCategory => Item.EquipSlotCategory.Value;

    [JsonIgnore] public IEnumerable<GearSetSlot> Slots => EquipSlotCategory.AvailableSlots();

    [JsonIgnore] public bool IsUnique => Item.IsUnique;

    [JsonIgnore] public IEnumerable<HrtMateria> Materia => _materia;

    [JsonIgnore] public int MateriaSlotCount => Item.MateriaSlotCount;

    [JsonIgnore]
    public int MaxMateriaSlots => Item.IsAdvancedMeldingPermitted ? 5 : Item.MateriaSlotCount;

    [JsonIgnore] public int StatCap => _statCapImpl.Value;

    public IEnumerable<StatType> StatTypesAffected
    {
        get
        {
            SortedSet<StatType> done = new();
            foreach (var stat in Item.BaseParam)
            {
                var type = (StatType)stat.Value.RowId;
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
                    result = Item.BaseParamSpecial.Zip(Item.BaseParamValueSpecial)
                                 .Where(x => x.First.RowId == (byte)type)
                                 .Aggregate(result, (current, param) => current + param.Second);

                result = Item.BaseParam.Zip(Item.BaseParamValue).Where(x => x.First.RowId == (byte)type)
                             .Aggregate(result, (current, param) => current + param.Second);
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
        MateriaLevel maxAllowed = ServiceManager.GameInfo.GetExpansionByLevel(Item.LevelEquip).MaxMateriaLevel;
        if (_materia.Count >= Item.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtItem : IEquatable<HrtItem>, IHrtDataType
{

    [JsonIgnore] protected static readonly ExcelSheet<Item> ItemSheet = ServiceManager.ExcelModule.GetSheet<Item>();
    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    private readonly uint _id;

    private Item? _itemCache;
    private LuminaItem? _luminaItemCache;

    [JsonIgnore] public Lazy<uint> LevelCache;

    public HrtItem(uint id)
    {
        _id = id;
        LevelCache = new Lazy<uint>(() => Item.LevelItem.RowId);
    }

    public virtual uint Id => _id;

    [JsonIgnore] public Rarity Rarity => (Rarity)Item.Rarity;

    [JsonIgnore] public ushort Icon => Item.Icon;

    protected LuminaItem? GameItem => _luminaItemCache ??= new LuminaItem(Item);

    protected Item Item => _itemCache ??= ItemSheet.GetRow(Id);

    public bool IsGear => this is GearItem || Item.ClassJobCategory.RowId != 0;
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

    public string Name => Item.Name.ExtractText() ?? "";
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
    private static readonly ExcelSheet<Materia> _materiaSheet = ServiceManager.ExcelModule.GetSheet<Materia>();

    [JsonIgnore] private static Lazy<Dictionary<uint, (MateriaCategory, MateriaLevel)>> _idLookupImpl = new(() =>
    {
        var result = new Dictionary<uint, (MateriaCategory, MateriaLevel)>();
        foreach (Materia materia in _materiaSheet)
        {
            int level = 0;
            foreach (var tier in materia.Item)
            {
                if (tier.RowId == 0) continue;
                result.Add(tier.RowId, ((MateriaCategory)materia.RowId, (MateriaLevel)level));
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
        _idCache = new Lazy<uint>(() => Materia?.Item[_materiaLevel].RowId ?? 0);
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