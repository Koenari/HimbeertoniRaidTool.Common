using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Item = HimbeertoniRaidTool.Common.Data.Item;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace HimbeertoniRaidTool.Common.Extensions;

public static class ItemExtensions
{
    private static ItemInfoService? _itemInfoService;
    internal static ItemInfoService ItemInfoService
    {
        private get => _itemInfoService ?? throw new InvalidOperationException("CommonLibrary is not initialized.");
        set => _itemInfoService = value;
    }
    public static bool CanBePurchased(this Item item) => ItemInfoService.CanBePurchased(item.Id);

    public static IEnumerable<(string shopName, SpecialShop.ItemStruct entry)> PurchasedFrom(this Item item) =>
        ItemInfoService.GetShopEntriesForItem(item.Id);

    public static LuminaItem AdjustItemCost(this RowRef<LuminaItem> itemId, ushort patchNumber) =>
        ItemInfoService.AdjustItemCost(itemId, patchNumber);

    public static bool IsTomeStone(this LuminaItem item, ushort patchNumber = 0) =>
        ItemInfoService.IsTomeStone(item.RowId, patchNumber);

    public static bool IsTomeStone(this Item item, ushort patchNumber = 0) =>
        ItemInfoService.IsTomeStone(item.Id, patchNumber);

    public static bool IsCurrency(this Item item) => ItemInfoService.IsCurrency(item.Id);

    public static bool IsCurrency(this LuminaItem item) => ItemInfoService.IsCurrency(item.RowId);

    public static bool CanBeLooted(this Item item) => ItemInfoService.CanBeLooted(item.Id);

    public static bool CanBeLooted(this LuminaItem item) => ItemInfoService.CanBeLooted(item.RowId);

    public static IEnumerable<InstanceWithLoot> LootSources(this Item item) =>
        ItemInfoService.GetLootSources(item.Id);

    public static IEnumerable<InstanceWithLoot> LootSources(this LuminaItem item) =>
        ItemInfoService.GetLootSources(item.RowId);

    public static ItemSource Source(this Item item) => ItemInfoService.GetSource(item);

    public static bool IsExchangableItem(this Item item) => ItemInfoService.UsedAsShopCurrency(item.Id);
    public static bool IsContainerItem(this Item item) => ItemInfoService.IsItemContainer(item.Id);

    public static IEnumerable<GearItem> PossiblePurchases(this Item item)
    {
        {
            if (IsExchangableItem(item))
                foreach (uint canBuy in ItemInfoService.GetPossiblePurchases(item.Id))
                {
                    yield return new GearItem(canBuy);
                }
            if (IsContainerItem(item))
                foreach (uint id in ItemInfoService.GetContainerContents(item.Id))
                {
                    yield return new GearItem(id);
                }
        }
    }
    public static bool IsFood(this LuminaItem item) => item.ItemAction.RowId != 0 && item.ItemAction.Value.Type == 845
     && item.ItemAction.Value.Data[0] == 48;

}