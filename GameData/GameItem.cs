using HimbeertoniRaidTool.Common.Extensions;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace HimbeertoniRaidTool.Common.GameData;

/// <summary>
///     Extends <see cref="Lumina.Excel.Sheets.Item" /> with enums and some additional logic
/// </summary>
public class GameItem(LuminaItem item)
{

    protected static readonly ExcelSheet<LuminaItem> ItemSheet = CommonLibrary.ExcelModule.GetSheet<LuminaItem>();

    private static readonly Lazy<Dictionary<uint, (MateriaCategory, MateriaLevel)>> _materiaLookupImpl = new(() =>
    {
        var result = new Dictionary<uint, (MateriaCategory, MateriaLevel)>
            { { 0, (MateriaCategory.None, MateriaLevel.None) } };
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

    private static readonly Lazy<Dictionary<(uint cat, MateriaLevel level), uint>> _reverseMateriaLookupImpl =
        new(() =>
        {
            var result = new Dictionary<(uint, MateriaLevel), uint>();
            foreach (var materia in CommonLibrary.ExcelModule.GetSheet<Materia>())
            {
                int level = 0;
                foreach (var tier in materia.Item)
                {
                    result.Add((materia.RowId, (MateriaLevel)level), tier.RowId);
                    level++;
                }
            }
            return result;
        });
    public static Dictionary<uint, (MateriaCategory cat, MateriaLevel level)> MateriaLookup => _materiaLookupImpl.Value;
    public static Dictionary<(uint, MateriaLevel), uint> ReverseMateriaLookup =>
        _reverseMateriaLookupImpl.Value;

    public GameItem(uint id) : this(ItemSheet.GetRow(id)) { }

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

    public IEnumerable<StatType> StatTypesAffected =>
        item.BaseParam.Where(stat => stat.RowId != 0).Select(stat => (StatType)stat.RowId);

    public int GetStat(StatType type, bool hq = false) => type switch
    {
        StatType.PhysicalDamage => item.DamagePhys,
        StatType.MagicalDamage  => item.DamageMag,
        StatType.Defense        => item.DefensePhys,
        StatType.MagicDefense   => item.DefenseMag,
        StatType.Delay          => item.Delayms,
        //ToDo: Some cases missing
        _ => item.BaseParam.Zip(item.BaseParamValue).Where(param => param.First.RowId == (uint)type)
                 .Aggregate(0, (current, param) => current + param.Second)
           + (CanBeHq && hq ? item.BaseParamSpecial.Zip(item.BaseParamValueSpecial)
                                  .Where(x => x.First.RowId == (uint)type)
                                  .Aggregate(0, (current, param) => current + param.Second) : 0),
    };
}