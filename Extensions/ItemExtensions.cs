using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Item = HimbeertoniRaidTool.Common.Data.Item;

namespace HimbeertoniRaidTool.Common.Extensions;

public static class ItemExtensions
{
    internal static ItemInfoService ItemInfoService
    {
        private get => field ?? throw new InvalidOperationException("CommonLibrary is not initialized.");
        set;
    }

    extension(Item item)
    {
        public bool CanBePurchased() => ItemInfoService.CanBePurchased(item.Id);
        public IEnumerable<(string shopName, SpecialShop.ItemStruct entry)> PurchasedFrom() =>
            ItemInfoService.GetShopEntriesForItem(item.Id);
        public bool IsTomeStone(ushort patchNumber = 0) =>
            ItemInfoService.IsTomeStone(item.Id, patchNumber);
        public bool IsCurrency() => ItemInfoService.IsCurrency(item.Id);
        public bool CanBeLooted() => ItemInfoService.CanBeLooted(item.Id);
        public IEnumerable<InstanceWithLoot> LootSources() =>
            ItemInfoService.GetLootSources(item.Id);
        public ItemSource Source() => ItemInfoService.GetSource(item);
        public bool IsExchangeableItem() => ItemInfoService.UsedAsShopCurrency(item.Id);
        public bool IsContainerItem() => ItemInfoService.IsItemContainer(item.Id);
        public IEnumerable<GearItem> PossiblePurchases()
        {
            foreach (uint canBuy in ItemInfoService.GetPossiblePurchases(item.Id))
            {
                yield return new GearItem(canBuy);
            }
            foreach (uint id in ItemInfoService.GetContainerContents(item.Id))
            {
                yield return new GearItem(id);
            }
        }
    }

    extension(RowRef<LuminaItem> item)
    {
        public LuminaItem AdjustItemCost(ushort patchNumber) =>
            ItemInfoService.AdjustItemCost(item, patchNumber);
    }

    extension(LuminaItem item)
    {
        public bool IsTomeStone(ushort patchNumber = 0) =>
            ItemInfoService.IsTomeStone(item.RowId, patchNumber);
        public bool IsCurrency() => ItemInfoService.IsCurrency(item.RowId);
        public bool CanBeLooted() => ItemInfoService.CanBeLooted(item.RowId);
        public IEnumerable<InstanceWithLoot> LootSources() =>
            ItemInfoService.GetLootSources(item.RowId);
        public bool IsFood() => item.ItemAction.RowId != 0
                             && item.ItemAction.Value.Action.RowId is 845 or 844
                             && item.ItemAction.Value.Data[0] == 48;
    }


}