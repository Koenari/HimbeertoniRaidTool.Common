using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HimbeertoniRaidTool.Common.Data;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
/// Extends <see cref="Lumina.Excel.GeneratedSheets.Item"/> by using enums where possible
/// </summary>
public class LuminaItem
{
    private Item _item;

    public Rarity Rarity => (Rarity)_item.Rarity;

    public LuminaItem(Item item)
    {
        _item = item;
    }
}