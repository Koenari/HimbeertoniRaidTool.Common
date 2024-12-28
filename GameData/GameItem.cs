using HimbeertoniRaidTool.Common.Extensions;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
///     Extends <see cref="Lumina.Excel.Sheets.Item" /> by using enums where possible
/// </summary>
public class GameItem(LuminaItem item)
{
    public LuminaItem RawItem => item;

    public uint ItemLevel => RawItem.LevelItem.RowId;
    public Rarity Rarity => (Rarity)item.Rarity;
    public bool IsFood => item.IsFood();
}