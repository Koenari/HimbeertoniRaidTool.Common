using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
///     Extends <see cref="Lumina.Excel.GeneratedSheets.Item" /> by using enums where possible
/// </summary>
public class LuminaItem(Item item)
{

    public Rarity Rarity => (Rarity)item.Rarity;
}