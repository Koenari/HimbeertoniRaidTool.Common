using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonDictionary]
public class Inventory : Dictionary<int, InventoryEntry>
{
    public bool Contains(uint id)
    {
        return Values.Any(i => i.ID == id);
    }

    public int IndexOf(uint id)
    {
        return this.FirstOrDefault(i => i.Value.ID == id).Key;
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
    public int quantity = 0;
    private string type;
    private HrtItem? hrtItem;
    private GearItem? gearItem;
    private HrtMateria? hrtMateria;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InventoryEntry(HrtItem item)
    {
        Item = item;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private InventoryEntry(string typeArg)
    {
        type = typeArg;
    }

    [JsonIgnore]
    public HrtItem Item
    {
        get
        {
            if (IsGear)
                return gearItem!;
            else if (IsMateria)
                return hrtMateria!;
            else
                return hrtItem!;
        }
        set
        {
            gearItem = null;
            hrtMateria = null;

            if (value is GearItem item)
            {
                gearItem = item;
                type = nameof(GearItem);
            }
            else if (value is HrtMateria mat)
            {
                hrtMateria = mat;
                type = nameof(HrtMateria);
            }
            else
            {
                hrtItem = value;
                type = nameof(HrtItem);
            }
        }
    }

    public bool IsGear => type == nameof(GearItem);
    public bool IsMateria => type == nameof(HrtMateria);

    public uint ID
    {
        get
        {
            return type switch
            {
                nameof(GearItem) => gearItem!.ID,
                nameof(HrtMateria) => hrtMateria!.ID,
                nameof(HrtItem) => hrtItem!.ID,
                _ => 0,
            };
        }
    }

    public static implicit operator InventoryEntry(HrtItem item)
    {
        return new InventoryEntry(item);
    }
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
    MGP = 29,
    TomestoneOfLaw = 30,
    TomestoneOfAstronomy = 43,
    TomestoneOfCausality = 44,
}