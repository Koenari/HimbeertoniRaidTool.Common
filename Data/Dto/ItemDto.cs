namespace HimbeertoniRaidTool.Common.Data.Dto;

public sealed class ItemDto(Item item)
{
    public readonly uint Id = item.Id;
    public static implicit operator ItemDto?(Item item)
    {
        return item.Filled ? item.ToDto() : null;
    }
}

public sealed class HqItemDto(HqItem item)
{
    public readonly uint Id = item.Id;
    public readonly bool Hq = item.IsHq;
    public static implicit operator HqItemDto?(HqItem? item)
    {
        return item?.Filled ?? false ? item.ToDto() : null;
    }
}

public sealed class GearItemDto(GearItem item)
{
    public readonly uint Id = item.Id;
    public readonly bool Hq = item.IsHq;
    public readonly IList<ItemDto>? Materia = item.Materia.Select(mat => new ItemDto(mat)).ToList();
    public readonly IDictionary<StatType, int>? RelicStats = item.RelicStats;
    public static implicit operator GearItemDto?(GearItem item)
    {
        return item.Filled ? item.ToDto() : null;
    }
}