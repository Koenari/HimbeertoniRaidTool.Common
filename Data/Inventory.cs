using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject]
public class Inventory : IEnumerable<KeyValuePair<int, InventoryEntry>>
{
    //Todo: Does not work on changes withing Inventory entry
    [JsonProperty("LastUpdated")] public DateTime LastUpdated { get; private set; }
    [JsonProperty("Data")] private Dictionary<int, InventoryEntry> _data = new();

    public InventoryEntry this[int id]
    {
        get => _data[id];
        set
        {
            LastUpdated = DateTime.UtcNow;
            _data[id] = value;
        }
    }
    public bool Contains(uint id) => _data.Values.Any(i => i.Id == id);

    public int ItemCount(uint id) => _data.Where(i => i.Value.Id == id).Sum(i => i.Value.Quantity);

    public int IndexOf(uint id) => _data.FirstOrDefault(i => i.Value.Id == id).Key;

    public void Remove(int idx)
    {
        LastUpdated = DateTime.UtcNow;
        _data.Remove(idx);
    }

    private int FirstFreeSlot()
    {
        for (int i = 0; i < _data.Values.Count; i++)
        {
            if (!_data.ContainsKey(i))
                return i;
        }
        return _data.Values.Count;
    }

    public int ReserveSlot(Item item, int quantity = 0)
    {
        if (Contains(item.Id)) return IndexOf(item.Id);
        int slot = FirstFreeSlot();
        this[slot] = new InventoryEntry(item)
        {
            Quantity = quantity,
        };
        return slot;
    }
    public IEnumerator<KeyValuePair<int, InventoryEntry>> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
}

[JsonObject]
public class Wallet : IEnumerable<KeyValuePair<Currency, int>>
{

    [JsonProperty("Data")] private Dictionary<Currency, int> _data = new();
    [JsonProperty("LastUpdated")] public DateTime LastUpdated { get; private set; }
    public bool ContainsKey(Currency key) => _data.ContainsKey(key);
    public int this[Currency cur]
    {
        get => _data[cur];
        set
        {
            LastUpdated = DateTime.UtcNow;
            _data[cur] = value;
        }
    }
    public ICollection<Currency> Keys => ((IDictionary<Currency, int>)_data).Keys;
    public ICollection<int> Values => ((IDictionary<Currency, int>)_data).Values;
    public IEnumerator<KeyValuePair<Currency, int>> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    public bool Contains(KeyValuePair<Currency, int> item) => _data.Contains(item);
    public bool Remove(KeyValuePair<Currency, int> item)
    {
        LastUpdated = DateTime.UtcNow;
        return _data.Remove(item.Key);
    }
    public int Count => _data.Count;
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore,
            MemberSerialization = MemberSerialization.Fields)]
public class InventoryEntry
{
    public int Quantity;
    private string _type;
    private Item? _hrtItem;
    private GearItem? _gearItem;
    private MateriaItem? _hrtMateria;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InventoryEntry(Item item)
    {
        Item = item;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private InventoryEntry(string typeArg)
    {
        _type = typeArg;
    }

    [JsonIgnore]
    public Item Item
    {
        get
        {
            if (IsGear)
                return _gearItem!;
            if (IsMateria)
                return _hrtMateria!;
            return _hrtItem!;
        }
        set
        {
            _gearItem = null;
            _hrtMateria = null;

            if (value is GearItem item)
            {
                _gearItem = item;
                _type = nameof(GearItem);
            }
            else if (value is MateriaItem mat)
            {
                _hrtMateria = mat;
                _type = nameof(MateriaItem);
            }
            else
            {
                _hrtItem = value;
                _type = nameof(Data.Item);
            }
        }
    }

    public bool IsGear => _type == nameof(GearItem);
    public bool IsMateria => _type == nameof(MateriaItem);

    public uint Id => _type switch
    {
        nameof(GearItem)    => _gearItem!.Id,
        nameof(MateriaItem) => _hrtMateria!.Id,
        nameof(Data.Item)   => _hrtItem!.Id,
        _                   => 0,
    };

    public static implicit operator InventoryEntry(Item item)
    {
        return new InventoryEntry(item);
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum Currency : uint
{
    Unknown = 0,
    Gil = 1,
    StormSeal = 20,
    SerpentSeal = 21,
    FlameSeal = 22,
    TomeStoneOfPhilosophy = 23,
    TomeStoneOfMythology = 24,
    WolfMark = 25,
    TomestoneOfSoldiery = 26,
    AlliedSeal = 27,
    TomestoneOfPoetics = 28,
    Mgp = 29,
    TomestoneOfLaw = 30,
    TomestoneOfEsoterics = 31,
    TomestoneOfLore = 32,
    TomestoneOfScripture = 33,
    TomestoneOfVerity = 34,
    TomestoneOfCreation = 35,
    TomestoneOfMendacity = 36,
    TomestoneOfGenesis = 37,
    TomestoneOfGoetia = 38,
    TomestoneOfPhantasmagoria = 39,
    TomestoneOfAllegory = 40,
    TomestoneOfRevelation = 41,
    TomestoneOfAphorism = 42,
    TomestoneOfAstronomy = 43,
    TomestoneOfCausality = 44,
    TomestoneOfComedy = 45,
    TomestoneOfHeliometry = 46,
}