using HimbeertoniRaidTool.Common.Data;
using Lumina.Excel.GeneratedSheets;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
///     Extends <see cref="Lumina.Excel.GeneratedSheets.Item" /> by using enums where possible
/// </summary>
public class LuminaItem
{
    private readonly Item _item;

    public LuminaItem(Item item)
    {
        _item = item;
    }

    public Rarity Rarity => (Rarity)_item.Rarity;
}