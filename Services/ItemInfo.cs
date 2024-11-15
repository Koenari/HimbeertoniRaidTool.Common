using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Services;

public class ItemInfo
{
    private readonly GameInfo _gameInfo;
    private readonly Dictionary<uint, ItemIdCollection> _itemContainerDb;
    private readonly Dictionary<uint, List<uint>> _lootSources;
    private readonly ExcelSheet<RecipeLookup> _recipeLookupSheet;
    private readonly Dictionary<uint, List<(uint shopID, int idx)>> _shopIndex;
    private readonly ExcelSheet<SpecialShop> _shopSheet;
    private readonly Dictionary<uint, List<uint>> _usedAsCurrency;

    internal ItemInfo(ExcelModule excelModule, CuratedData curData, GameInfo gameInfo)
    {
        _gameInfo = gameInfo;
        _itemContainerDb = curData.ItemContainerDb;
        _shopSheet = excelModule.GetSheet<SpecialShop>();
        _recipeLookupSheet = excelModule.GetSheet<RecipeLookup>();
        //Load Vendor Data
        _shopIndex = new Dictionary<uint, List<(uint shopID, int idx)>>();
        _usedAsCurrency = new Dictionary<uint, List<uint>>();
        foreach (SpecialShop shop in _shopSheet.Where(s => !s.Name.IsEmpty))
        {
            for (int idx = 0; idx < shop.Item.Count; idx++)
            {
                SpecialShop.ItemStruct entry = shop.Item[idx];
                foreach (SpecialShop.ItemStruct.ReceiveItemsStruct receiveEntry in entry.ReceiveItems)
                {
                    if (receiveEntry.Item.RowId == 0)
                        continue;
                    if (!_shopIndex.ContainsKey(receiveEntry.Item.RowId))
                        _shopIndex[receiveEntry.Item.RowId] = [];
                    if (_shopIndex[receiveEntry.Item.RowId].Contains((shop.RowId, idx)))
                        continue;
                    _shopIndex[receiveEntry.Item.RowId].Add((shop.RowId, idx));
                    foreach (SpecialShop.ItemStruct.ItemCostsStruct item in entry.ItemCosts)
                    {
                        if (item.ItemCost.RowId == 0) continue;
                        uint costId = TranslateTomestoneIds(item.ItemCost.RowId, entry.PatchNumber);
                        _usedAsCurrency.TryAdd(costId, []);
                        _usedAsCurrency[costId].Add(entry.ReceiveItems[0].Item.RowId);
                    }
                }
            }
        }
        _lootSources = new Dictionary<uint, List<uint>>();
        foreach (InstanceWithLoot instance in _gameInfo.GetInstances())
        {
            foreach (HrtItem loot in instance.AllLoot)
            {
                RegisterLoot(loot.Id, instance.InstanceId);
                if (!IsItemContainer(loot.Id)) continue;
                foreach (HrtItem item in GetContainerContents(loot.Id).Select(id => new HrtItem(id)))
                {
                    RegisterLoot(item.Id, instance.InstanceId);
                }
            }
        }
    }
    private static uint TranslateTomestoneIds(uint id, ushort patchNumber) => (patchNumber, id) switch
    {
        (600, 2) => 43,
        (620, 2) => 44,
        (640, 3) => 45,
        (700, 2) => 46,
        (700, 3) => 47,
        _        => id,
    };

    private void RegisterLoot(uint itemId, uint instanceId)
    {
        if (!_lootSources.ContainsKey(itemId))
            _lootSources[itemId] = new List<uint>();
        if (!_lootSources[itemId].Contains(instanceId))
            _lootSources[itemId].Add(instanceId);
    }

    public bool CanBeLooted(uint itemId) => _lootSources.ContainsKey(itemId);

    public IEnumerable<InstanceWithLoot> GetLootSources(uint itemId)
    {
        if (_lootSources.TryGetValue(itemId, out var instanceIDs))
            foreach (uint instanceId in instanceIDs)
            {
                yield return _gameInfo.GetInstance(instanceId);
            }
    }

    public bool IsItemContainer(uint itemId) => _itemContainerDb.ContainsKey(itemId);

    public static bool IsCurrency(uint itemId) => Enum.IsDefined((Currency)itemId);

    public bool UsedAsShopCurrency(uint itemId) => _usedAsCurrency.ContainsKey(itemId);

    public bool CanBePurchased(uint itemId) => _shopIndex.ContainsKey(itemId);

    public bool CanBeCrafted(uint itemId) => _recipeLookupSheet.HasRow(itemId);

    public ItemIdCollection GetPossiblePurchases(uint itemId) =>
        new ItemIdList(_usedAsCurrency.GetValueOrDefault(itemId) ?? Enumerable.Empty<uint>());

    public ItemIdCollection GetContainerContents(uint itemId) =>
        _itemContainerDb.GetValueOrDefault(itemId, ItemIdCollection.Empty);

    public IEnumerable<(string shopName, SpecialShop.ItemStruct entry)> GetShopEntriesForItem(uint itemId)
    {
        if (_shopIndex.TryGetValue(itemId, out var shopEntries))
            foreach ((uint shopId, int idx) in shopEntries)
            {
                SpecialShop shop = _shopSheet.GetRow(shopId);
                yield return (shop.Name.ToString(), shop.Item[idx]);
            }
    }

    public static bool IsTomeStone(uint itemId, ushort patch = 0) => TranslateTomestoneIds(itemId, patch) switch
    {
        23              => true,
        24              => true,
        26              => true,
        28              => true,
        >= 30 and <= 50 => true,
        _               => false,
    };

    public ItemSource GetSource(HrtItem item, int maxDepth = 10)
    {
        uint itemId = item.Id;
        if (itemId == 0 || --maxDepth < 0)
            return ItemSource.Undefined;
        if (item.Rarity == Rarity.Relic)
            return ItemSource.Relic;
        if (_lootSources.TryGetValue(itemId, out var instanceId))
            return _gameInfo.GetInstance(instanceId.First()).InstanceType.ToItemSource();
        if (CanBeCrafted(itemId))
            return ItemSource.Crafted;
        if (CanBePurchased(itemId))
        {
            if (GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Any(e => CanBeCrafted(e.ItemCost.RowId)))
             || GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Where(e => e.ItemCost.RowId != 0)
                                                          .Any(e => GetSource(new HrtItem(e.ItemCost.RowId), maxDepth)
                                                                 == ItemSource.Crafted)))
                return ItemSource.Crafted;
            if (GetShopEntriesForItem(itemId)
                    .Any(se => se.entry.ItemCosts.Any(e => IsTomeStone(e.ItemCost.RowId, se.entry.PatchNumber)))
             || GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Where(e => e.ItemCost.RowId != 0)
                                                          .Any(e => GetSource(new HrtItem(e.ItemCost.RowId), maxDepth)
                                                                 == ItemSource.Tome)))
                return ItemSource.Tome;
            if (GetShopEntriesForItem(itemId).Any(se =>
                                                      se.entry.ItemCosts.Any(
                                                          e => GetSource(new HrtItem(e.ItemCost.RowId), maxDepth)
                                                            == ItemSource.Raid)))
                return ItemSource.Raid;
            return ItemSource.Shop;
        }

        return ItemSource.Undefined;
    }
}