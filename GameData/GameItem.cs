using HimbeertoniRaidTool.Common.Extensions;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

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

    public uint ItemLevel => item.LevelItem.RowId;

    public Rarity Rarity => (Rarity)item.Rarity;

    public bool IsFood => item.IsFood();

    public bool IsGear => item.ClassJobCategory.RowId != 0;

    public bool IsMateria => MateriaLookup.ContainsKey(item.RowId);

    public IEnumerable<Job> ApplicableJobs => item.ClassJobCategory.Value.ToJob();

    public EquipSlotCategory EquipSlotCategory => item.EquipSlotCategory.Value;

    public bool IsUnique => item.IsUnique;

    public int MateriaSlotCount => item.MateriaSlotCount;

    public bool IsAdvancedMeldingPermitted => item.IsAdvancedMeldingPermitted;

    public int LevelEquip => item.LevelEquip;

    public bool CanBeHq => item.CanBeHq;

    public ushort Icon => item.Icon;

    public ReadOnlySeString Name => item.Name;

    public ItemAction ItemAction => item.ItemAction.Value;

    public IEnumerable<StatType> StatTypesAffected => item.BaseParam.Select(stat => (StatType)stat.Value.RowId);

    public int GetStat(StatType type, bool hq = false) => type switch
    {
        StatType.PhysicalDamage => item.DamagePhys,
        StatType.MagicalDamage  => item.DamageMag,
        StatType.Defense        => item.DefensePhys,
        StatType.MagicDefense   => item.DefenseMag,
        StatType.Delay          => item.Delayms,
        //ToDo: Some cases missing
        _ => item.BaseParam.Zip(item.BaseParamValue).Where(x => x.First.RowId == (byte)type)
                 .Aggregate(0, (current, param) => current + param.Second)
           + item.BaseParamSpecial.Zip(item.BaseParamValueSpecial).Where(x => hq && x.First.RowId == (byte)type)
                 .Aggregate(0, (current, param) => current + param.Second),
    };
}