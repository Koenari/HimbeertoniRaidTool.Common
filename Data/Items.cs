using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.GameData;
using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class GearItem : HqItem
{
    [JsonIgnore] public static readonly new GearItem Empty = new();

    [JsonProperty("Materia")] private readonly List<MateriaItem> _materia = new();

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
            if (GameItem.RawItem.Rarity == (byte)Rarity.Relic) return int.MaxValue;
            int maxVal = int.Max(GameItem.RawItem.BaseParamValue[2], GameItem.RawItem.BaseParamValue[3]);
            if (IsHq)
                maxVal += int.Max(GameItem.RawItem.BaseParamValueSpecial[2], GameItem.RawItem.BaseParamValueSpecial[3]);
            return maxVal;
        });
    }
    [JsonIgnore] public new string DataTypeName => CommonLoc.DataTypeName_item_gear;

    [JsonIgnore] public IEnumerable<Job> Jobs => GameItem.RawItem.ClassJobCategory.Value.ToJob();

    [JsonIgnore] public EquipSlotCategory EquipSlotCategory => GameItem.RawItem.EquipSlotCategory.Value;

    [JsonIgnore] public IEnumerable<GearSetSlot> Slots => EquipSlotCategory.AvailableSlots();

    [JsonIgnore] public bool IsUnique => GameItem.RawItem.IsUnique;

    [JsonIgnore] public IEnumerable<MateriaItem> Materia => _materia;

    [JsonIgnore] public int MateriaSlotCount => GameItem.RawItem.MateriaSlotCount;

    [JsonIgnore]
    public int MaxMateriaSlots => GameItem.RawItem.IsAdvancedMeldingPermitted ? 5 : GameItem.RawItem.MateriaSlotCount;

    [JsonIgnore] public int StatCap => _statCapImpl.Value;

    public IEnumerable<StatType> StatTypesAffected
    {
        get
        {
            SortedSet<StatType> done = [];
            foreach (var stat in GameItem.RawItem.BaseParam)
            {
                var type = (StatType)stat.Value.RowId;
                done.Add(type);
            }
            if (IsRelic() && RelicStats is not null)
            {
                foreach ((var type, int value) in RelicStats)
                {
                    if (value > 0)
                        done.Add(type);
                }
            }
            foreach (var mat in Materia)
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
                result += GameItem.RawItem.DamagePhys;
                break;
            case StatType.MagicalDamage:
                result += GameItem.RawItem.DamageMag;
                break;
            case StatType.Defense:
                result += GameItem.RawItem.DefensePhys;
                break;
            case StatType.MagicDefense:
                result += GameItem.RawItem.DefenseMag;
                break;
            case StatType.Delay:
                result += GameItem.RawItem.Delayms;
                break;
            default:
                if (IsHq)
                    result = GameItem.RawItem.BaseParamSpecial.Zip(GameItem.RawItem.BaseParamValueSpecial)
                                     .Where(x => x.First.RowId == (byte)type)
                                     .Aggregate(result, (current, param) => current + param.Second);

                result = GameItem.RawItem.BaseParam.Zip(GameItem.RawItem.BaseParamValue)
                                 .Where(x => x.First.RowId == (byte)type)
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
        Dictionary<MateriaItem, int> cnt = new();
        foreach (var s in _materia)
        {
            if (!cnt.TryAdd(s, 1))
                cnt[s]++;
        }
        foreach (var s in other._materia)
        {
            if (cnt.ContainsKey(s))
                cnt[s]--;
            else
                return false;
        }

        return cnt.Values.All(s => s == 0);
    }
    public bool IsRelic() => GameItem.Rarity == Rarity.Relic;
    public bool CanAffixMateria() => _materia.Count < MaxMateriaSlots;

    public void AddMateria(MateriaItem materiaItem)
    {
        if (!CanAffixMateria()) return;
        _materia.Add(materiaItem);
        InvalidateCache();
    }

    public void RemoveMateria(int removeAt)
    {
        if (removeAt >= _materia.Count) return;
        _materia.RemoveAt(removeAt);
        InvalidateCache();
    }

    public void ReplaceMateria(int index, MateriaItem newMat)
    {
        if (index >= _materia.Count) return;
        _materia[index] = newMat;
        InvalidateCache();
    }

    //ToDo: Do from Lumina
    public MateriaLevel MaxAffixableMateriaLevel()
    {
        if (!CanAffixMateria()) return 0;
        var maxAllowed = GameInfo.GetExpansionByLevel(GameItem.RawItem.LevelEquip).MaxMateriaLevel;
        if (_materia.Count >= GameItem.RawItem.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }
}

public class HqItem : Item, IEquatable<HqItem>
{
    public HqItem(uint id) : base(id) { }
    [JsonProperty] public bool IsHq { get; init; }
    public bool Equals(HqItem? other) => IsHq == other?.IsHq && base.Equals(other);

    public override bool Equals(object? obj) => Equals(obj as HqItem);
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), IsHq);
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Item : IEquatable<Item>, IHrtDataType
{
    public static readonly Item Empty = new(0);
    [JsonIgnore]
    protected static readonly ExcelSheet<Lumina.Excel.Sheets.Item> ItemSheet =
        CommonLibrary.ExcelModule.GetSheet<Lumina.Excel.Sheets.Item>();
    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    private readonly uint _id;

    private Lumina.Excel.Sheets.Item? _itemCache;
    private GameItem? _luminaItemCache;

    [JsonIgnore] public Lazy<uint> LevelCache;

    public Item(uint id)
    {
        _id = id;
        LevelCache = new Lazy<uint>(() => GameItem.ItemLevel);
    }

    public virtual uint Id => _id;

    [JsonIgnore] public Rarity Rarity => GameItem.Rarity;

    [JsonIgnore] public ushort Icon => GameItem.RawItem.Icon;

    public bool CanBeHq => GameItem.RawItem.CanBeHq;

    protected GameItem GameItem => _luminaItemCache ??= new GameItem(ItemSheet.GetRow(Id));

    public bool IsGear => this is GearItem || GameItem.RawItem.ClassJobCategory.RowId != 0;

    public bool IsFood => GameItem.IsFood;

    [JsonIgnore] public uint ItemLevel => LevelCache.Value;

    public bool Filled => Id > 0;

    public bool Equals(Item? obj) => Id == obj?.Id;

    public string Name => GameItem.RawItem.Name.ExtractText();
    [JsonIgnore] public string DataTypeName => CommonLoc.DataTypeName_item;
    public override string ToString() => Name;
    public override bool Equals(object? obj) => Equals(obj as Item);

    public override int GetHashCode() => Id.GetHashCode();
}

[JsonObject(MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class MateriaItem : Item, IEquatable<MateriaItem>
{
    [JsonIgnore]
    private static readonly ExcelSheet<Materia> MateriaSheet =
        CommonLibrary.ExcelModule.GetSheet<Materia>();

    [JsonIgnore] private static Lazy<Dictionary<uint, (MateriaCategory, MateriaLevel)>> _idLookupImpl = new(() =>
    {
        var result = new Dictionary<uint, (MateriaCategory, MateriaLevel)>();
        foreach (var materia in MateriaSheet)
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

    public MateriaItem(uint id) : this(IdLookup[id]) { }
    public MateriaItem() : this(0, 0)
    {
    }

    public MateriaItem((MateriaCategory cat, MateriaLevel lvl) mat) : this(mat.cat, mat.lvl)
    {
    }

    [JsonConstructor]
    public MateriaItem(MateriaCategory cat, MateriaLevel lvl) : base(0)
    {
        Category = cat;
        _materiaLevel = (byte)lvl;
        _idCache = new Lazy<uint>(() => LuminaMateria.Item[_materiaLevel].RowId);
    }
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_materia;
    [JsonIgnore] public new string DataTypeName => DataTypeNameStatic;
    [JsonIgnore] public MateriaLevel Level => (MateriaLevel)_materiaLevel;
    [JsonIgnore] public override uint Id => _idCache.Value;
    private Materia LuminaMateria => MateriaSheet.GetRow((ushort)Category);
    public StatType StatType => Category.GetStatType();

    public bool Equals(MateriaItem? other) => base.Equals(other);

    public int GetStat() => LuminaMateria.Value[_materiaLevel];
}

[JsonObject(MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class FoodItem : HqItem
{
    private static readonly ExcelSheet<ItemAction> ItemActionSheet = CommonLibrary.ExcelModule.GetSheet<ItemAction>();
    private static readonly ExcelSheet<ItemFood> FoodSheet = CommonLibrary.ExcelModule.GetSheet<ItemFood>();
    private readonly ItemFood? _luminaFood;
    public FoodItem(uint id) : base(id)
    {
        var action = GameItem.RawItem.ItemAction;
        if (GameItem.IsFood)
            _luminaFood = FoodSheet.GetRow(action.Value.Data[1]);
        IsHq = true;
    }

    public int ApplyEffect(StatType type, int before)
    {
        if (_luminaFood is null) return before;
        foreach (var param in _luminaFood.Value.Params.Where(param => param.BaseParam.RowId == (uint)type))
        {
            int added;
            if (param.IsRelative)
                added = before * (IsHq ? param.ValueHQ : param.Value) / 100;
            else
                added = IsHq ? param.ValueHQ : param.Value;
            added = int.Clamp(added, 0, IsHq ? param.MaxHQ : param.Max);
            return before + added;
        }
        return before;
    }
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
    public static implicit operator ItemIdList(uint[] ids)
    {
        return new ItemIdList(ids);
    }
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

    public static implicit operator ItemIdCollection(uint id)
    {
        return new ItemIdList(id);
    }

    public static implicit operator ItemIdCollection(Range range)
    {
        return new ItemIdRange(range.Start.Value, range.End.Value);
    }

    public static implicit operator ItemIdCollection((uint, uint) id)
    {
        return new ItemIdRange(id.Item1, id.Item2);
    }
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