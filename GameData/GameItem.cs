using HimbeertoniRaidTool.Common.Extensions;
using Lumina.Excel.Sheets;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
///     Extends <see cref="Lumina.Excel.Sheets.Item" /> by using enums where possible
/// </summary>
public class GameItem(LuminaItem item)
{
    private static readonly Lazy<Dictionary<uint, (MateriaCategory, MateriaLevel)>> MateriaLookupImpl = new(() =>
    {
        var result = new Dictionary<uint, (MateriaCategory, MateriaLevel)>();
        foreach (var materia in CommonLibrary.ExcelModule.GetSheet<Materia>())
        {
            int level = 0;
            foreach (var tier in materia.Item.Where(tier => tier.RowId != 0))
            {
                result.Add(tier.RowId, ((MateriaCategory)materia.RowId, (MateriaLevel)level));
                level++;
            }
        }
        return result;
    });
    public static Dictionary<uint, (MateriaCategory, MateriaLevel)> MateriaLookup => MateriaLookupImpl.Value;
    public LuminaItem RawItem => item;

    public uint ItemLevel => item.LevelItem.RowId;
    public Rarity Rarity => (Rarity)item.Rarity;
    public bool IsFood => item.IsFood();
    public bool IsGear => item.ClassJobCategory.RowId != 0;
    public bool IsMateria => MateriaLookup.ContainsKey(item.RowId);
}