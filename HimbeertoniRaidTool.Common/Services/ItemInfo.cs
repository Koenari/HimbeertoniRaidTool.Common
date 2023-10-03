using System;
using HimbeertoniRaidTool.Common.Data;
using Lumina.Excel;
using Lumina.Excel.CustomSheets;
using System.Collections.Generic;
using System.Linq;

namespace HimbeertoniRaidTool.Common.Services;

public class ItemInfo
{
    private readonly GameInfo _gameInfo;
    private readonly ExcelSheet<SpecialShop> _shopSheet;
    private readonly ExcelSheet<Lumina.Excel.GeneratedSheets.RecipeLookup> _recipeLookupSheet;
    private readonly Dictionary<uint, ItemIDCollection> _itemContainerDb;
    private readonly Dictionary<uint, List<(uint shopID, int idx)>> _shopIndex;
    private readonly Dictionary<uint, List<uint>> _usedAsCurrency;
    private readonly Dictionary<uint, List<uint>> _lootSources;

    internal ItemInfo(ExcelModule excelModule, CuratedData curData, GameInfo gameInfo)
    {
        _gameInfo = gameInfo;
        _itemContainerDb = curData.ItemContainerDB;
        _shopSheet = excelModule.GetSheet<SpecialShop>()!;
        _recipeLookupSheet = excelModule.GetSheet<Lumina.Excel.GeneratedSheets.RecipeLookup>()!;
        //Load Vendor Data
        _shopIndex = new Dictionary<uint, List<(uint shopID, int idx)>>();
        _usedAsCurrency = new Dictionary<uint, List<uint>>();
        foreach (SpecialShop shop in _shopSheet.Where(s => !string.IsNullOrEmpty(s.Name.RawString)))
            for (int idx = 0; idx < shop.ShopEntries.Length; idx++)
            {
                SpecialShop.ShopEntry entry = shop.ShopEntries[idx];
                foreach (SpecialShop.ItemReceiveEntry receiveEntry in entry.ItemReceiveEntries)
                {
                    if (receiveEntry.Item.Row == 0)
                        continue;
                    if (!_shopIndex.ContainsKey(receiveEntry.Item.Row))
                        _shopIndex[receiveEntry.Item.Row] = new List<(uint shopID, int idx)>();
                    if (_shopIndex[receiveEntry.Item.Row].Contains((shop.RowId, idx)))
                        continue;
                    _shopIndex[receiveEntry.Item.Row].Add((shop.RowId, idx));
                    foreach (SpecialShop.ItemCostEntry item in entry.ItemCostEntries)
                    {
                        if (item.Item.Row == 0) continue;
                        if (!_usedAsCurrency.ContainsKey(item.Item.Row))
                            _usedAsCurrency.Add(item.Item.Row, new List<uint>());
                        _usedAsCurrency[item.Item.Row].Add(entry.ItemReceiveEntries[0].Item.Row);
                    }
                }
            }

        _lootSources = new Dictionary<uint, List<uint>>();
        foreach (InstanceWithLoot instance in _gameInfo.GetInstances())
        foreach (HrtItem loot in instance.AllLoot)
        {
            RegisterLoot(loot.ID, instance.InstanceID);
            if (!IsItemContainer(loot.ID)) continue;
            foreach (HrtItem item in GetContainerContents(loot.ID).Select(id => new HrtItem(id)))
                RegisterLoot(item.ID, instance.InstanceID);
        }
    }

    private void RegisterLoot(uint itemID, uint instanceID)
    {
        if (!_lootSources.ContainsKey(itemID))
            _lootSources[itemID] = new List<uint>();
        if (!_lootSources[itemID].Contains(instanceID))
            _lootSources[itemID].Add(instanceID);
    }

    public bool CanBeLooted(uint itemID)
    {
        return _lootSources.ContainsKey(itemID);
    }

    public IEnumerable<InstanceWithLoot> GetLootSources(uint itemID)
    {
        if (_lootSources.TryGetValue(itemID, out var instanceIDs))
            foreach (uint instanceID in instanceIDs)
                yield return _gameInfo.GetInstance(instanceID);
    }

    public bool IsItemContainer(uint itemID)
    {
        return _itemContainerDb.ContainsKey(itemID);
    }

    public static bool IsCurrency(uint itemId)
    {
        return Enum.IsDefined((Currency)itemId);
    }

    public bool UsedAsShopCurrency(uint itemID)
    {
        return _usedAsCurrency.ContainsKey(itemID);
    }

    public bool CanBePurchased(uint itemID)
    {
        return _shopIndex.ContainsKey(itemID);
    }

    public bool CanBeCrafted(uint itemID)
    {
        return _recipeLookupSheet.GetRow(itemID) != null;
    }

    public ItemIDCollection GetPossiblePurchases(uint itemID)
    {
        return new ItemIDList(_usedAsCurrency.GetValueOrDefault(itemID) ?? Enumerable.Empty<uint>());
    }

    public ItemIDCollection GetContainerContents(uint itemID)
    {
        return _itemContainerDb.GetValueOrDefault(itemID, ItemIDCollection.Empty);
    }

    public IEnumerable<(string shopName, SpecialShop.ShopEntry entry)> GetShopEntriesForItem(uint itemID)
    {
        if (_shopIndex.TryGetValue(itemID, out var shopEntries))
            foreach ((uint shopID, int idx) in shopEntries)
            {
                SpecialShop? shop = _shopSheet.GetRow(shopID);
                if (shop != null)
                    yield return (shop.Name, shop.ShopEntries[idx]);
            }
    }

    public static bool IsTomeStone(uint itemID)
    {
        return itemID switch
        {
            23 => true,
            24 => true,
            26 => true,
            28 => true,
            >= 30 and <= 45 => true,
            _ => false,
        };
    }

    public ItemSource GetSource(HrtItem item, int maxDepth = 10)
    {
        uint itemID = item.ID;
        maxDepth--;
        if (itemID == 0)
            return ItemSource.undefined;
        if (maxDepth < 0) return ItemSource.undefined;
        if (item.Rarity == Rarity.Relic)
            return ItemSource.Relic;
        if (_lootSources.TryGetValue(itemID, out var instanceID))
            return _gameInfo.GetInstance(instanceID.First()).InstanceType.ToItemSource();
        if (CanBeCrafted(itemID))
            return ItemSource.Crafted;
        if (CanBePurchased(itemID))
        {
            if (GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Any(e => CanBeCrafted(e.Item.Row)))
                || GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Where(e => e.Item.Row != 0)
                    .Any(e => GetSource(new HrtItem(e.Item.Row), maxDepth) == ItemSource.Crafted)))
                return ItemSource.Crafted;
            if (GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Any(e => IsTomeStone(e.Item.Row)))
                || GetShopEntriesForItem(itemID).Any(se => se.entry.ItemCostEntries.Where(e => e.Item.Row != 0)
                    .Any(e => GetSource(new HrtItem(e.Item.Row), maxDepth) == ItemSource.Tome)))
                return ItemSource.Tome;
            if (GetShopEntriesForItem(itemID).Any(se =>
                    se.entry.ItemCostEntries.Any(e => GetSource(new HrtItem(e.Item.Row), maxDepth) == ItemSource.Raid)))
                return ItemSource.Raid;
            return ItemSource.Shop;
        }

        return ItemSource.undefined;
    }
}