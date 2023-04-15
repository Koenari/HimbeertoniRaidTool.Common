using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Data;
using Lumina.Excel;
using Lumina.Excel.CustomSheets;

namespace HimbeertoniRaidTool.Common.Services;

public class ItemInfo
{
    private readonly GameInfo _gameInfo;
    private readonly ExcelSheet<SpecialShop> ShopSheet;
    private readonly ExcelSheet<Lumina.Excel.GeneratedSheets.RecipeLookup> RecipeLookupSheet;
    private readonly Dictionary<uint, ItemIDCollection> ItemContainerDB;
    private readonly Dictionary<uint, List<(uint shopID, int idx)>> ShopIndex;
    private readonly Dictionary<uint, List<uint>> UsedAsCurrency;
    private readonly Dictionary<uint, List<uint>> LootSources;
    internal ItemInfo(ExcelModule excelModule, CuratedData curData, GameInfo gameInfo)
    {
        _gameInfo = gameInfo;
        ItemContainerDB = curData.ItemContainerDB;
        ShopSheet = excelModule.GetSheet<SpecialShop>()!;
        RecipeLookupSheet = excelModule.GetSheet<Lumina.Excel.GeneratedSheets.RecipeLookup>()!;
        //Load Vendor Data
        ShopIndex = new();
        UsedAsCurrency = new();
        foreach (SpecialShop shop in ShopSheet.Where(s => !string.IsNullOrEmpty(s.Name.RawString)))
        {
            for (int idx = 0; idx < shop.ShopEntries.Length; idx++)
            {
                var entry = shop.ShopEntries[idx];
                for (int receiveIdx = 0; receiveIdx < entry.ItemReceiveEntries.Length; receiveIdx++)
                {
                    if (entry.ItemReceiveEntries[receiveIdx].Item.Row == 0)
                        continue;
                    if (!ShopIndex.ContainsKey(entry.ItemReceiveEntries[receiveIdx].Item.Row))
                        ShopIndex[entry.ItemReceiveEntries[receiveIdx].Item.Row] = new();
                    if (ShopIndex[entry.ItemReceiveEntries[receiveIdx].Item.Row].Contains((shop.RowId, idx)))
                        continue;
                    ShopIndex[entry.ItemReceiveEntries[receiveIdx].Item.Row].Add((shop.RowId, idx));
                    foreach (var item in entry.ItemCostEntries)
                    {
                        if (item.Item.Row == 0) continue;
                        if (!UsedAsCurrency.ContainsKey(item.Item.Row))
                            UsedAsCurrency.Add(item.Item.Row, new());
                        UsedAsCurrency[item.Item.Row].Add(entry.ItemReceiveEntries[0].Item.Row);
                    }
                }
            }
        }
        LootSources = new();
        foreach (InstanceWithLoot instance in _gameInfo.GetInstances())
            foreach (HrtItem loot in instance.AllLoot)
            {
                RegisterLoot(loot.ID, instance.InstanceID);
                if (IsItemContainer(loot.ID))
                    foreach (HrtItem item in GetContainerContents(loot.ID).Select(id => new HrtItem(id)))
                        RegisterLoot(item.ID, instance.InstanceID);
            }
    }
    private void RegisterLoot(uint itemID, uint instanceID)
    {
        if (!LootSources.ContainsKey(itemID))
            LootSources[itemID] = new();
        if (!LootSources[itemID].Contains(instanceID))
            LootSources[itemID].Add(instanceID);
    }
    public bool CanBeLooted(uint itemID) => LootSources.ContainsKey(itemID);
    public IEnumerable<InstanceWithLoot> GetLootSources(uint itemID)
    {
        if (this.LootSources.TryGetValue(itemID, out List<uint>? instanceIDs))
            foreach (uint instanceID in instanceIDs)
                yield return _gameInfo.GetInstance(instanceID);
    }
    public bool IsItemContainer(uint itemID) => ItemContainerDB.ContainsKey(itemID);
    public bool UsedAsShopCurrency(uint itemID) => UsedAsCurrency.ContainsKey(itemID);
    public bool CanBePurchased(uint itemID) => ShopIndex.ContainsKey(itemID);
    public bool CanbBeCrafted(uint itemID) => RecipeLookupSheet.GetRow(itemID) != null;
    public ItemIDCollection GetPossiblePurchases(uint itemID) => new ItemIDList(UsedAsCurrency.GetValueOrDefault(itemID) ?? Enumerable.Empty<uint>());
    public ItemIDCollection GetContainerContents(uint itemID) => ItemContainerDB.GetValueOrDefault(itemID, ItemIDCollection.Empty);
    public IEnumerable<(string shopName, SpecialShop.ShopEntry entry)> GetShopEntriesForItem(uint itemID)
    {
        if (ShopIndex.TryGetValue(itemID, out var shopEntries))
        {
            foreach (var (shopID, idx) in shopEntries)
            {
                var shop = ShopSheet.GetRow(shopID);
                if (shop != null)
                    yield return (shop.Name, shop.ShopEntries[idx]);
            }
        }

    }

    public static bool IsTomeStone(uint itemID) => itemID switch
    {
        23 => true,
        24 => true,
        26 => true,
        28 => true,
        >= 30 and <= 44 => true,
        _ => false,
    };
    public ItemSource GetSource(HrtItem item, int maxDepth = 10)
    {
        uint itemID = item.ID;
        maxDepth--;
        if (itemID == 0)
            return ItemSource.undefined;
        if (maxDepth < 0)
        {
            return ItemSource.undefined;
        }
        if (item.Item?.Rarity == 4)
            return ItemSource.Relic;
        if (LootSources.TryGetValue(itemID, out List<uint>? instanceID))
            return _gameInfo.GetInstance(instanceID.First()).InstanceType.ToItemSource();
        if (CanbBeCrafted(itemID))
            return ItemSource.Crafted;
        if (CanBePurchased(itemID))
        {
            if (GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Any(e => CanbBeCrafted(e.Item.Row)))
                 || GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Where(e => e.Item.Row != 0)
                        .Any(e => GetSource(new(e.Item.Row), maxDepth) == ItemSource.Crafted)))
                return ItemSource.Crafted;
            if (GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Any(e => IsTomeStone(e.Item.Row)))
                 || GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Where(e => e.Item.Row != 0)
                        .Any(e => GetSource(new(e.Item.Row), maxDepth) == ItemSource.Tome)))
                return ItemSource.Tome;
            if ((GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Any(e => GetSource(new(e.Item.Row), maxDepth) == ItemSource.Raid))))
                return ItemSource.Raid;
            return ItemSource.Shop;
        }
        return ItemSource.undefined;
    }
}
