using Lumina.Excel;
using Lumina.Excel.Sheets;
using Item = HimbeertoniRaidTool.Common.Data.Item;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace HimbeertoniRaidTool.Common.Services;

internal class ItemInfoService
{
    private readonly Dictionary<uint, ItemIdCollection> _itemContainerDb;
    private readonly Dictionary<uint, List<uint>> _lootSources;
    private readonly ExcelSheet<RecipeLookup> _recipeLookupSheet;
    private readonly Dictionary<uint, List<(uint shopID, int idx)>> _shopIndex;
    private readonly ExcelSheet<SpecialShop> _shopSheet;
    private readonly ExcelSheet<LuminaItem> _itemSheet;
    private readonly Dictionary<uint, List<uint>> _usedAsCurrency;

    internal ItemInfoService(ExcelModule excelModule)
    {
        _itemContainerDb = new CuratedData().ItemContainerDb;
        _shopSheet = excelModule.GetSheet<SpecialShop>();
        _recipeLookupSheet = excelModule.GetSheet<RecipeLookup>();
        _itemSheet = excelModule.GetSheet<Lumina.Excel.Sheets.Item>();
        //Load Vendor Data
        _shopIndex = new Dictionary<uint, List<(uint shopID, int idx)>>();
        _usedAsCurrency = new Dictionary<uint, List<uint>>();
        foreach (var shop in _shopSheet.Where(s => !s.Name.IsEmpty))
        {
            for (int idx = 0; idx < shop.Item.Count; idx++)
            {
                var entry = shop.Item[idx];
                foreach (var receiveEntry in entry.ReceiveItems)
                {
                    if (receiveEntry.Item.RowId == 0)
                        continue;
                    if (!_shopIndex.ContainsKey(receiveEntry.Item.RowId))
                        _shopIndex[receiveEntry.Item.RowId] = [];
                    if (_shopIndex[receiveEntry.Item.RowId].Contains((shop.RowId, idx)))
                        continue;
                    _shopIndex[receiveEntry.Item.RowId].Add((shop.RowId, idx));
                    foreach (var item in entry.ItemCosts)
                    {
                        if (item.ItemCost.RowId == 0) continue;
                        uint costId = AdjustItemCostId(item.ItemCost.RowId, entry.PatchNumber);
                        _usedAsCurrency.TryAdd(costId, []);
                        _usedAsCurrency[costId].Add(entry.ReceiveItems[0].Item.RowId);
                    }
                }
            }
        }
        _lootSources = new Dictionary<uint, List<uint>>();
        foreach (var instance in GameInfo.GetInstances())
        {
            foreach (var loot in instance.AllLoot)
            {
                RegisterLoot(loot.Id, instance.InstanceId);
                if (!IsItemContainer(loot.Id)) continue;
                foreach (var item in GetContainerContents(loot.Id).Select(id => new Item(id)))
                {
                    RegisterLoot(item.Id, instance.InstanceId);
                }
            }
        }
    }

    public Lumina.Excel.Sheets.Item AdjustItemCost(RowRef<Lumina.Excel.Sheets.Item> cost, ushort patchNumber)
        => _itemSheet.GetRow(AdjustItemCostId(cost.RowId, patchNumber));


    public static uint AdjustItemCostId(uint itemId, ushort patch) => (patch, itemId) switch
    {
        (600, 2) => 43,
        (620, 2) => 44,
        (640, 3) => 45,
        (700, 2) => 46,
        (700, 3) => 47,
        _        => itemId,
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
                yield return GameInfo.GetInstance(instanceId);
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
                var shop = _shopSheet.GetRow(shopId);
                yield return (shop.Name.ToString(), shop.Item[idx]);
            }
    }

    public static bool IsTomeStone(uint itemId, ushort patch = 0) => AdjustItemCostId(itemId, patch) switch
    {
        23              => true,
        24              => true,
        26              => true,
        28              => true,
        >= 30 and <= 50 => true,
        _               => false,
    };

    public ItemSource GetSource(Item item, int maxDepth = 10)
    {
        uint itemId = item.Id;
        if (itemId == 0 || --maxDepth < 0)
            return ItemSource.Undefined;
        if (item.Rarity == Rarity.Relic)
            return ItemSource.Relic;
        if (_lootSources.TryGetValue(itemId, out var instanceId))
            return GameInfo.GetInstance(instanceId.First()).InstanceType.ToItemSource();
        if (CanBeCrafted(itemId))
            return ItemSource.Crafted;
        if (CanBePurchased(itemId))
        {
            if (GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Any(e => CanBeCrafted(e.ItemCost.RowId)))
             || GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Where(e => e.ItemCost.RowId != 0)
                                                          .Any(e => GetSource(new Item(e.ItemCost.RowId), maxDepth)
                                                                 == ItemSource.Crafted)))
                return ItemSource.Crafted;
            if (GetShopEntriesForItem(itemId)
                    .Any(se => se.entry.ItemCosts.Any(e => IsTomeStone(e.ItemCost.RowId, se.entry.PatchNumber)))
             || GetShopEntriesForItem(itemId).Any(se => se.entry.ItemCosts.Where(e => e.ItemCost.RowId != 0)
                                                          .Any(e => GetSource(new Item(e.ItemCost.RowId), maxDepth)
                                                                 == ItemSource.Tome)))
                return ItemSource.Tome;
            if (GetShopEntriesForItem(itemId).Any(se =>
                                                      se.entry.ItemCosts.Any(
                                                          e => GetSource(new Item(e.ItemCost.RowId), maxDepth)
                                                            == ItemSource.Raid)))
                return ItemSource.Raid;
            return ItemSource.Shop;
        }

        return ItemSource.Undefined;
    }
}