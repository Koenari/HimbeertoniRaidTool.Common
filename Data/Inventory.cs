using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HimbeertoniRaidTool.Common.Data.Dto;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
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

[JsonObject(MemberSerialization.OptIn)]
public class Wallet : IEnumerable<KeyValuePair<Currency, int>>, IHasDtoIsCreatableAndUpdatable<Wallet, WalletDto>
{
    #region Serialized

    [JsonProperty("Data")] private Dictionary<Currency, int> _data = new();
    [JsonProperty("LastUpdated")] public DateTime LastUpdated { get; private set; }

    #endregion


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
    public WalletDto ToDto() => new(this);
    public void UpdateFromDto(WalletDto dto)
    {
        _data = dto.Data;
        LastUpdated = dto.LastUpdated;

    }
    public static Wallet FromDto(WalletDto dto) => new()
    {
        _data = dto.Data,
        LastUpdated = dto.LastUpdated,
    };
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore,
            MemberSerialization = MemberSerialization.OptIn)]
public class InventoryEntry
{
    #region Serialized

    [JsonProperty] public int Quantity;
    [JsonProperty] private string _type = string.Empty;
    [JsonProperty] private Item? _hrtItem;
    [JsonProperty] private GearItem? _gearItem;
    [JsonProperty] private MateriaItem? _hrtMateria;

    #endregion
    public InventoryEntry(Item item)
    {
        Item = item;
    }

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
            _hrtItem = null;
            _gearItem = null;
            _hrtMateria = null;

            switch (value)
            {
                case GearItem item:
                    _gearItem = item;
                    _type = nameof(GearItem);
                    break;
                case MateriaItem mat:
                    _hrtMateria = mat;
                    _type = nameof(MateriaItem);
                    break;
                default:
                    _hrtItem = value;
                    _type = nameof(Data.Item);
                    break;
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