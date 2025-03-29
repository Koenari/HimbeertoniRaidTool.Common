using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HimbeertoniRaidTool.Common.GameData;
using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public class HqItem : Item, IEquatable<HqItem>
{
    private readonly bool _hq;
    [JsonProperty] public bool IsHq
    {
        get => _hq && CanBeHq;
        init => _hq = value & CanBeHq;
    }

    [JsonConstructor]
    public HqItem(uint id, bool hq = false) : base(id)
    {
        IsHq = hq;
    }

    public bool Equals(HqItem? other) => IsHq == other?.IsHq && base.Equals(other);

    public override string ToString() => Name + (IsHq ? " (HQ)" : string.Empty);

    public override bool Equals(object? obj) => Equals(obj as HqItem);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), IsHq);
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class Item : IEquatable<Item>, IHrtDataType
{
    public static readonly Item Empty = new(0);

    #region Serialized

    [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.Include)]
    public readonly uint Id;

    #endregion

    private Lazy<GameItem> _gameItem;

    [JsonConstructor]
    public Item(uint id)
    {
        Id = id;
        //Needed hack for json deserialization
        _gameItem = new Lazy<GameItem>(() => new GameItem(Id));
    }

    public Rarity Rarity => GameItem.Rarity;

    public ushort Icon => GameItem.Icon;

    public bool CanBeHq => GameItem.CanBeHq;

    public IEnumerable<Job> Jobs => GameItem.ApplicableJobs;

    public EquipSlotCategory EquipSlotCategory => GameItem.EquipSlotCategory;

    public bool IsUnique => GameItem.IsUnique;

    public int MateriaSlotCount => GameItem.MateriaSlotCount;

    protected GameItem GameItem => _gameItem.Value;

    public bool IsGear => this is GearItem || GameItem.IsGear;

    public bool IsFood => this is FoodItem || GameItem.IsFood;

    public bool IsMateria => this is MateriaItem || GameItem.IsMateria;

    public uint ItemLevel => GameItem.ItemLevel;

    public bool Filled => Id > 0;

    public bool Equals(Item? obj) => Id == obj?.Id;

    public string Name => GameItem.Name.ExtractText();

    public string DataTypeName => CommonLoc.DataTypeName_item;

    public override string ToString() => Name;

    public override bool Equals(object? obj) => Equals(obj as Item);

    public override int GetHashCode() => Id.GetHashCode();
}

public class ItemIdCollection : IEnumerable<uint>
{
    public static readonly ItemIdCollection Empty = new();
    private readonly Collection<uint> _ids;

    public ItemIdCollection(params uint[] ids)
    {
        _ids = new Collection<uint>(ids.ToList());
    }

    public ItemIdCollection(Range range, params uint[] ids)
    {
        _ids = new Collection<uint>(ToList(range).Concat(ids).ToList());
    }

    public ItemIdCollection(IEnumerable<uint> ids)
    {
        _ids = new Collection<uint>(ids.ToList());
    }

    public void Add(uint id) => _ids.Add(id);

    public IEnumerator<uint> GetEnumerator() => _ids.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _ids.GetEnumerator();

    public static implicit operator ItemIdCollection(uint id)
    {
        return new ItemIdCollection(id);
    }

    public static implicit operator ItemIdCollection(Range range)
    {
        return new ItemIdCollection(range);
    }

    private static List<uint> ToList(Range range) => Enumerable
                                                     .Range(range.Start.Value,
                                                            Math.Max(0, range.End.Value - range.Start.Value + 1))
                                                     .ToList().ConvertAll(x => (uint)x);
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