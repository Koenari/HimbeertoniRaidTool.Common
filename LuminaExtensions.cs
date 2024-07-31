using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

namespace HimbeertoniRaidTool.Common;

public static class TribeExtensions
{
    public static int GetRacialModifier(this Tribe t, StatType type) => type switch
    {
        StatType.Strength     => t.STR,
        StatType.Dexterity    => t.DEX,
        StatType.Vitality     => t.VIT,
        StatType.Intelligence => t.INT,
        StatType.Mind         => t.MND,
        _                     => 0,
    };
}

public static class EquipSlotCategoryExtensions
{
    public static bool Contains(this EquipSlotCategory? self, GearSetSlot? slot) => self.HasValueAt(slot, 1);

    public static bool Disallows(this EquipSlotCategory? self, GearSetSlot? slot) => self.HasValueAt(slot, -1);

    public static bool HasValueAt(this EquipSlotCategory? self, GearSetSlot? slot, sbyte value)
    {
        if (self == null)
            return false;
        return slot switch
        {
            GearSetSlot.MainHand    => self.MainHand == value,
            GearSetSlot.Head        => self.Head == value,
            GearSetSlot.Body        => self.Body == value,
            GearSetSlot.Hands       => self.Gloves == value,
            GearSetSlot.Waist       => self.Waist == value,
            GearSetSlot.Legs        => self.Legs == value,
            GearSetSlot.Feet        => self.Feet == value,
            GearSetSlot.OffHand     => self.OffHand == value,
            GearSetSlot.Ear         => self.Ears == value,
            GearSetSlot.Neck        => self.Neck == value,
            GearSetSlot.Wrist       => self.Wrists == value,
            GearSetSlot.Ring1       => self.FingerR == value,
            GearSetSlot.Ring2       => self.FingerL == value,
            GearSetSlot.SoulCrystal => self.SoulCrystal == value,
            _                       => false,
        };
    }

    public static IEnumerable<GearSetSlot> AvailableSlots(this EquipSlotCategory? self)
    {
        for (int i = 0; i < (int)GearSetSlot.SoulCrystal; i++)
        {
            if (self?.Contains((GearSetSlot)i) ?? false)
                yield return (GearSetSlot)i;
        }
    }
}

public static class ClassJobCategoryExtensions
{
    public static IEnumerable<Job> ToJob(this ClassJobCategory self) => Enum.GetValues<Job>().Where(self.Contains);

    public static bool Contains(this ClassJobCategory? cat, Job job) => cat is not null && job switch
    {
        Job.ADV => cat.ADV,
        Job.AST => cat.AST,
        Job.BLM => cat.BLM,
        Job.BLU => cat.BLU,
        Job.BRD => cat.BRD,
        Job.DNC => cat.DNC,
        Job.DRG => cat.DRG,
        Job.DRK => cat.DRK,
        Job.GNB => cat.GNB,
        Job.MCH => cat.MCH,
        Job.MNK => cat.MNK,
        Job.NIN => cat.NIN,
        Job.PLD => cat.PLD,
        Job.RDM => cat.RDM,
        Job.RPR => cat.RPR,
        Job.SAM => cat.SAM,
        Job.SCH => cat.SCH,
        Job.SGE => cat.SGE,
        Job.SMN => cat.SMN,
        Job.WAR => cat.WAR,
        Job.WHM => cat.WHM,
        Job.GLA => cat.GLA,
        Job.MRD => cat.MRD,
        Job.LNC => cat.LNC,
        Job.PGL => cat.PGL,
        Job.ARC => cat.ARC,
        Job.THM => cat.THM,
        Job.ACN => cat.ACN,
        Job.CNJ => cat.CNJ,
        Job.ROG => cat.ROG,
        Job.VPR => cat.VPR,
        Job.PCT => cat.PCT,
        _       => false,
    };
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Sheet("SpecialShop", 0xb29819b0)]
public class SpecialShop : ExcelRow
{
    public const int NUM_ENTRIES = 60;
    public const int GLOBAL_OFFSET = 1;
    public const int NUM_RECEIVE = 2;
    public const int RECEIVE_SIZE = 4;
    public const int NUM_COST = 3;
    public const int COST_SIZE = 4;
    public const int ADDITIONAL_SHOPENTRY_FIELDS = 7;
    public const int COST_OFFSET = GLOBAL_OFFSET + NUM_ENTRIES * NUM_RECEIVE * RECEIVE_SIZE; //481
    public const int AFTER_COST_OFFSET = COST_OFFSET + NUM_ENTRIES * NUM_COST * COST_SIZE; //481+720 = 1201

    public const int
        AFTER_ENTRIES_OFFSET =
            AFTER_COST_OFFSET + NUM_ENTRIES * ADDITIONAL_SHOPENTRY_FIELDS + 360; //1201 + 420 = 1621

    //ToDO: Fix
    public const int PATCH_NUMBER_OFFSET = 1981;

    public SeString Name { get; set; }
    public ShopEntry[] ShopEntries = new ShopEntry[NUM_ENTRIES];
    public byte UseCurrencyType { get; set; }
    public LazyRow<Quest> UnlockQuest { get; set; }
    public LazyRow<DefaultTalk> CompleteText { get; set; }
    public LazyRow<DefaultTalk> NotCompleteText { get; set; }
    public uint UnknownData1625 { get; set; }
    public bool UnknownData1626 { get; set; }
    public ushort UnknownData1627 { get; set; }
    public uint UnknownData1628 { get; set; }
    public bool UnknownData1629 { get; set; }

    public class ShopEntry
    {
        public ItemReceiveEntry[] ItemReceiveEntries { get; set; } = new ItemReceiveEntry[NUM_RECEIVE];
        public ItemCostEntry[] ItemCostEntries { get; set; } = new ItemCostEntry[NUM_COST];
        public LazyRow<Quest> Quest { get; set; }
        public int UnknownData1261 { get; set; }
        public byte UnknownData1321 { get; set; }
        public byte UnknownData1381 { get; set; }

        public LazyRow<Achievement> AchievementUnlock { get; set; }

        //Achievement related??
        //HAs entries for Triple Triad Cards, Whistles/Horns, Current Tome Gear(100)
        public byte UnknownData1501 { get; set; }
        public ushort PatchNumber { get; set; }
    }

    public class ItemReceiveEntry
    {
        public LazyRow<Item> Item { get; set; }
        public uint Count { get; set; }
        public LazyRow<SpecialShopItemCategory> SpecialShopItemCategory { get; set; }
        public bool Hq { get; set; }
    }

    public class ItemCostEntry
    {
        public LazyRow<Item> Item { get; set; }
        public uint Count { get; set; }
        public bool Hq { get; set; }
        public ushort CollectabilityRatingCost { get; set; }
    }

    public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Name = parser.ReadColumn<SeString>(0)!;
        for (int i = 0; i < ShopEntries.Length; i++)
        {
            ShopEntries[i] = new ShopEntry();
            //Receives
            for (int j = 0; j < NUM_RECEIVE; j++)
            {
                ShopEntries[i].ItemReceiveEntries[j] = new ItemReceiveEntry
                {
                    Item = new LazyRow<Item>(gameData,
                                             parser.ReadColumn<int>(GLOBAL_OFFSET + j * NUM_ENTRIES * RECEIVE_SIZE
                                                                  + 0 * NUM_ENTRIES +
                                                                    i), language),
                    Count = parser.ReadColumn<uint>(GLOBAL_OFFSET + j * NUM_ENTRIES * RECEIVE_SIZE +
                                                    1 * NUM_ENTRIES + i),
                    SpecialShopItemCategory =
                        new LazyRow<SpecialShopItemCategory>(gameData,
                                                             parser.ReadColumn<int>(
                                                                 GLOBAL_OFFSET + j * NUM_ENTRIES * RECEIVE_SIZE +
                                                                 2 * NUM_ENTRIES + i), language),
                    Hq = parser.ReadColumn<bool>(GLOBAL_OFFSET + j * NUM_ENTRIES * RECEIVE_SIZE + 3 * NUM_ENTRIES +
                                                 i),
                };
            }
            //COSTS
            for (int j = 0; j < NUM_COST; j++)
            {
                ShopEntries[i].ItemCostEntries[j] = new ItemCostEntry
                {
                    Item = new LazyRow<Item>(gameData,
                                             parser.ReadColumn<int>(
                                                 COST_OFFSET + j * NUM_ENTRIES * COST_SIZE + 0 * NUM_ENTRIES + i),
                                             language),
                    Count =
                        parser.ReadColumn<uint>(COST_OFFSET + j * NUM_ENTRIES * COST_SIZE + 1 * NUM_ENTRIES + i),
                    Hq = parser.ReadColumn<bool>(COST_OFFSET + j * NUM_ENTRIES * COST_SIZE + 2 * NUM_ENTRIES + i),
                    CollectabilityRatingCost =
                        parser.ReadColumn<ushort>(COST_OFFSET + j * NUM_ENTRIES * COST_SIZE + 3 * NUM_ENTRIES + i),
                };
            }
            ShopEntries[i].Quest =
                new LazyRow<Quest>(gameData, parser.ReadColumn<int>(AFTER_COST_OFFSET + 0 * NUM_ENTRIES + i),
                                   language);
            ShopEntries[i].UnknownData1261 = parser.ReadColumn<int>(AFTER_COST_OFFSET + 1 * NUM_ENTRIES + i);
            ShopEntries[i].UnknownData1321 = parser.ReadColumn<byte>(AFTER_COST_OFFSET + 2 * NUM_ENTRIES + i);
            ShopEntries[i].UnknownData1381 = parser.ReadColumn<byte>(AFTER_COST_OFFSET + 3 * NUM_ENTRIES + i);

            ShopEntries[i].AchievementUnlock =
                new LazyRow<Achievement>(gameData, parser.ReadColumn<int>(AFTER_COST_OFFSET + 4 * NUM_ENTRIES + i),
                                         language);
            ShopEntries[i].UnknownData1501 = parser.ReadColumn<byte>(AFTER_COST_OFFSET + 5 * NUM_ENTRIES + i);
            ShopEntries[i].PatchNumber =
                parser.ReadColumn<ushort>(PATCH_NUMBER_OFFSET + i); //(AFTER_COST_OFFSET + 6 * NUM_ENTRIES + i);
            if (ShopEntries[i].PatchNumber == 600)
                foreach (ItemCostEntry? item in ShopEntries[i].ItemCostEntries)
                    //Astronomy TomeStone
                {
                    if (item.Item.Row == 2)
                        item.Item = new LazyRow<Item>(gameData, 43, language);
                }
            if (ShopEntries[i].PatchNumber == 620)
                foreach (ItemCostEntry? item in ShopEntries[i].ItemCostEntries)
                {
                    switch (item.Item.Row)
                    {
                        case 2:
                            item.Item = new LazyRow<Item>(gameData, 44, language);
                            break; //White crafter scrip
                        //case 3: item.Item = new(gameData, 44, language); break; //Casualty TomeStone
                        //case 4: item.Item = new(gameData, 25200, language); break; //White gatherer scrip
                        //case 6: item.Item = new(gameData, 33913, language); break; //Purple crafter scrip
                        //case 7: item.Item = new(gameData, 33914, language); break; //Purple gatherer scrip
                    }
                }

            if (ShopEntries[i].PatchNumber == 640)
                foreach (ItemCostEntry? item in ShopEntries[i].ItemCostEntries)
                {
                    switch (item.Item.Row)
                    {
                        //case 2: item.Item = new(gameData, 25199, language); break; //White crafter scrip
                        case 3:
                            item.Item = new LazyRow<Item>(gameData, 45, language);
                            break; //Casualty TomeStone
                        //case 4: item.Item = new(gameData, 25200, language); break; //White gatherer scrip
                        //case 6: item.Item = new(gameData, 33913, language); break; //Purple crafter scrip
                        //case 7: item.Item = new(gameData, 33914, language); break; //Purple gatherer scrip
                    }
                }
            if (ShopEntries[i].PatchNumber == 700)
                foreach (ItemCostEntry? item in ShopEntries[i].ItemCostEntries)
                {
                    switch (item.Item.Row)
                    {
                        case 2:
                            item.Item = new LazyRow<Item>(gameData, 46, language);
                            break; //Allagan tomestone of aesthetics
                        case 3:
                            item.Item = new LazyRow<Item>(gameData, 47, language);
                            break; //Allagan tomestone of heliometry
                        //case 4: item.Item = new(gameData, 25200, language); break; //White gatherer scrip
                        //case 6: item.Item = new(gameData, 33913, language); break; //Purple crafter scrip
                        //case 7: item.Item = new(gameData, 33914, language); break; //Purple gatherer scrip
                    }
                }
        }

        UseCurrencyType = parser.ReadColumn<byte>(AFTER_ENTRIES_OFFSET);
        UnlockQuest = new LazyRow<Quest>(gameData, parser.ReadColumn<int>(AFTER_ENTRIES_OFFSET + 1), language);
        CompleteText =
            new LazyRow<DefaultTalk>(gameData, parser.ReadColumn<int>(AFTER_ENTRIES_OFFSET + 2), language);
        NotCompleteText =
            new LazyRow<DefaultTalk>(gameData, parser.ReadColumn<int>(AFTER_ENTRIES_OFFSET + 3), language);
        UnknownData1625 = parser.ReadColumn<uint>(AFTER_ENTRIES_OFFSET + 4);
        UnknownData1626 = parser.ReadColumn<bool>(AFTER_ENTRIES_OFFSET + 5);
        UnknownData1627 = parser.ReadColumn<ushort>(AFTER_ENTRIES_OFFSET + 6);
        UnknownData1628 = parser.ReadColumn<uint>(AFTER_ENTRIES_OFFSET + 7);
        UnknownData1629 = parser.ReadColumn<bool>(AFTER_ENTRIES_OFFSET + 8);
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.