using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonDictionary]
public class Inventory : Dictionary<int, InventoryEntry>
{
    public bool Contains(uint id)
    {
        return Values.Any(i => i.Id == id);
    }

    public int ItemCount(uint id)
    {
        return this.Where(i => i.Value.Id == id).Sum(i => i.Value.Quantity);
    }

    public int IndexOf(uint id)
    {
        return this.FirstOrDefault(i => i.Value.Id == id).Key;
    }

    public int FirstFreeSlot()
    {
        for (int i = 0; i < Values.Count; i++)
            if (!ContainsKey(i))
                return i;
        return Values.Count;
    }
}

[JsonDictionary]
public class Wallet : Dictionary<Currency, uint>
{
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore,
    MemberSerialization = MemberSerialization.Fields)]
public class InventoryEntry
{
    public int Quantity = 0;
    private string _type;
    private HrtItem? _hrtItem;
    private GearItem? _gearItem;
    private HrtMateria? _hrtMateria;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InventoryEntry(HrtItem item)
    {
        Item = item;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private InventoryEntry(string typeArg)
    {
        _type = typeArg;
    }

    [JsonIgnore]
    public HrtItem Item
    {
        get
        {
            if (IsGear)
                return _gearItem!;
            else if (IsMateria)
                return _hrtMateria!;
            else
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
            else if (value is HrtMateria mat)
            {
                _hrtMateria = mat;
                _type = nameof(HrtMateria);
            }
            else
            {
                _hrtItem = value;
                _type = nameof(HrtItem);
            }
        }
    }

    public bool IsGear => _type == nameof(GearItem);
    public bool IsMateria => _type == nameof(HrtMateria);

    public uint Id
    {
        get
        {
            return _type switch
            {
                nameof(GearItem) => _gearItem!.Id,
                nameof(HrtMateria) => _hrtMateria!.Id,
                nameof(HrtItem) => _hrtItem!.Id,
                _ => 0,
            };
        }
    }

    public static implicit operator InventoryEntry(HrtItem item) => new(item);
}

public enum Currency : uint
{
    Unknown = 0,
    Gil = 1,
    TomeStoneOfPhilosophy = 23,
    TomeStoneOfMythology = 24,
    WolfMark = 25,
    TomestoneOfSoldiery = 26,
    AlliedSeal = 27,
    TomestoneOfPoetics = 28,
    Mgp = 29,
    TomestoneOfLaw = 30,
    TomestoneOfAstronomy = 43,
    TomestoneOfCausality = 44,
}