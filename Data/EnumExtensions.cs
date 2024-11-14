using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public static class EnumExtensions
{
    private static ExcelSheet<ClassJob>? _jobSheetCache;
    private static readonly Dictionary<Job, ClassJob?> _jobCache = new();
    private static ExcelSheet<ClassJob> JobSheet => _jobSheetCache ??= ServiceManager.ExcelModule.GetSheet<ClassJob>();

    private static ClassJob GetClassJob(Job j)
    {
        _jobCache.TryAdd(j, null);
        return _jobCache[j] ??= JobSheet.GetRow((uint)j);
    }

    public static Role GetRole(this Job c)
    {
        ClassJob cj = GetClassJob(c);
        if (cj.PartyBonus > 4)
            return (Role)cj.PartyBonus;
        return (Role)cj.Role;
    }

    public static bool IsCombatRole(this Role role) =>
        role is Role.Tank or Role.Healer or Role.Melee or Role.Caster or Role.Ranged;

    public static StatType MainStat(this Job job) => (StatType)GetClassJob(job).PrimaryStat;

    public static int GroupSize(this GroupType groupType) => groupType switch
    {
        GroupType.Solo  => 1,
        GroupType.Group => 4,
        GroupType.Raid  => 8,
        _               => 0,
    };

    public static bool CanHaveShield(this Job job) => job is Job.PLD or Job.THM or Job.GLA or Job.CNJ;

    public static bool IsCombatJob(this Job j) => j is < Job.CRP or > Job.FSH;

    public static bool IsDoH(this Job j) => j is >= Job.MIN and <= Job.FSH;

    public static bool IsDoL(this Job j) => j is >= Job.CRP and <= Job.CUL;

    public static Job GetBaseJob(this Job j) => (Job)GetClassJob(j).ClassJobParent.RowId;

    public static bool HasBaseJob(this Job j) => GetBaseJob(j) is not Job.ADV;
    public static StatType GetStatType(this MateriaCategory materiaCategory) => materiaCategory switch
    {
        MateriaCategory.Piety         => StatType.Piety,
        MateriaCategory.DirectHit     => StatType.DirectHitRate,
        MateriaCategory.CriticalHit   => StatType.CriticalHit,
        MateriaCategory.Determination => StatType.Determination,
        MateriaCategory.Tenacity      => StatType.Tenacity,
        MateriaCategory.Gathering     => StatType.Gathering,
        MateriaCategory.Perception    => StatType.Perception,
        MateriaCategory.Gp            => StatType.GP,
        MateriaCategory.Craftsmanship => StatType.Craftsmanship,
        MateriaCategory.Cp            => StatType.CP,
        MateriaCategory.Control       => StatType.Control,
        MateriaCategory.SkillSpeed    => StatType.SkillSpeed,
        MateriaCategory.SpellSpeed    => StatType.SpellSpeed,
        _                             => StatType.None,
    };

    public static ItemSource ToItemSource(this InstanceType contentType) => contentType switch
    {
        InstanceType.Raid    => ItemSource.Raid,
        InstanceType.Trial   => ItemSource.Trial,
        InstanceType.Dungeon => ItemSource.Dungeon,
        _                    => ItemSource.Undefined,
    };

    public static bool IsSecondary(this StatType statType) => statType switch
    {
        StatType.CriticalHit   => true,
        StatType.Determination => true,
        StatType.DirectHitRate => true,
        StatType.SpellSpeed    => true,
        StatType.SkillSpeed    => true,
        StatType.Piety         => true,
        StatType.Tenacity      => true,
        StatType.Craftsmanship => true,
        StatType.Control       => true,
        StatType.Gathering     => true,
        StatType.Perception    => true,
        _                      => false,
    };
    public static string PrefixName(this MateriaCategory cat)
    {
        HrtMateria mat = new(cat, MateriaLevel.I);
        return mat.Name.Length > 9 ? mat.Name[..^9] : "";
    }

    public static string FriendlyName(this StatType t) => t switch
    {
        StatType.Strength            => CommonLoc.StatType_Strength,
        StatType.Dexterity           => CommonLoc.StatType_Dexterity,
        StatType.Vitality            => CommonLoc.StatType_Vitality,
        StatType.Intelligence        => CommonLoc.StatType_Intelligence,
        StatType.Mind                => CommonLoc.StatType_Mind,
        StatType.Piety               => CommonLoc.StatType_Piety,
        StatType.HP                  => CommonLoc.StatType_HP,
        StatType.MP                  => CommonLoc.StatType_MP,
        StatType.PhysicalDamage      => CommonLoc.StatType_PhysicalDamage,
        StatType.MagicalDamage       => CommonLoc.StatType_MagicDamage,
        StatType.Tenacity            => CommonLoc.StatType_Tenacity,
        StatType.AttackPower         => CommonLoc.StatType_AttackPower,
        StatType.Defense             => CommonLoc.StatType_Defense,
        StatType.DirectHitRate       => CommonLoc.StatType_Direct_Hit,
        StatType.MagicDefense        => CommonLoc.StatType_MagicalDefense,
        StatType.CriticalHitPower    => CommonLoc.StatType_Critical_Hit_Power,
        StatType.CriticalHit         => CommonLoc.StatType_Critical_Hit,
        StatType.AttackMagicPotency  => CommonLoc.StatType_AttackMagicPotency,
        StatType.HealingMagicPotency => CommonLoc.StatType_HealingMagicPotency,
        StatType.Determination       => CommonLoc.StatType_Determination,
        StatType.SkillSpeed          => CommonLoc.StatType_Skill_Speed,
        StatType.SpellSpeed          => CommonLoc.StatType_SpellSpeed,
        _                            => t.ToString(),
    };

    public static string AlternativeFriendlyName(this StatType t) => t switch
    {
        StatType.CriticalHit                       => CommonLoc.StatType_CriticalDamage,
        StatType.SpellSpeed or StatType.SkillSpeed => CommonLoc.StatType_SpsSksMultiplier,
        _                                          => CommonLoc.undefined,
    };

    public static string Abbrev(this StatType t) => t switch
    {
        StatType.CriticalHit   => CommonLoc.StatTypeAbbrev_CriticalHit,
        StatType.DirectHitRate => CommonLoc.StatTypeAbbrev_DirectHit,
        StatType.SkillSpeed    => CommonLoc.StatTypeAbbrev_SkillSpeed,
        StatType.SpellSpeed    => CommonLoc.StatTypeAbbrev_SpellSpeed,
        StatType.Determination => CommonLoc.StatTypeAbbrev_Determination,
        StatType.Piety         => CommonLoc.StatTypeAbbrev_Piety,
        StatType.Mind          => CommonLoc.StatTypeAbbrev_Mind,
        StatType.Strength      => CommonLoc.StatTypeAbbrev_Strength,
        StatType.Dexterity     => CommonLoc.StatTypeAbbrev_Dexterity,
        StatType.Intelligence  => CommonLoc.StatTypeAbbrev_Intelligence,
        StatType.Vitality      => CommonLoc.StatTypeAbbrev_Vitality,
        StatType.Tenacity      => CommonLoc.StatTypeAbbrev_Tenacity,
        _                      => "XXX",
    };

    public static string FriendlyName(this GearSetManager manager) => manager switch
    {
        GearSetManager.Hrt     => CommonLoc.GearManager_HrtLocal,
        GearSetManager.Etro    => CommonLoc.GearManager_etro_gg,
        GearSetManager.XivGear => CommonLoc.GearManager_XivGear,
        _                      => CommonLoc.undefined,
    };

    public static string FriendlyName(this GroupType groupType) => groupType switch
    {
        GroupType.Solo  => CommonLoc.GroupType_Solo,
        GroupType.Group => CommonLoc.GroupType_Group,
        GroupType.Raid  => CommonLoc.GroupType_FullGroup,
        _               => CommonLoc.undefined,
    };

    public static string FriendlyName(this GearSetSlot slot, bool detailed = false) => (slot, detailed) switch
    {
        (GearSetSlot.MainHand, _)                       => CommonLoc.GearSlot_Weapon,
        (GearSetSlot.OffHand, _)                        => CommonLoc.GearSlot_Shield,
        (GearSetSlot.Head, _)                           => CommonLoc.GearSlot_Head,
        (GearSetSlot.Body, _)                           => CommonLoc.GearSlot_Body,
        (GearSetSlot.Hands, _)                          => CommonLoc.GearSlot_Gloves,
        (GearSetSlot.Waist, _)                          => CommonLoc.GearSlot_Belt,
        (GearSetSlot.Legs, _)                           => CommonLoc.GearSlot_Trousers,
        (GearSetSlot.Feet, _)                           => CommonLoc.GearSlot_Shoes,
        (GearSetSlot.Ear, _)                            => CommonLoc.GearSlot_Earrings,
        (GearSetSlot.Neck, _)                           => CommonLoc.GearSlot_Necklace,
        (GearSetSlot.Wrist, _)                          => CommonLoc.GearSlot_Bracelet,
        (GearSetSlot.Ring1, true)                       => CommonLoc.GearSlot_Ring_R,
        (GearSetSlot.Ring2, true)                       => CommonLoc.GearSlot_Ring_L,
        (GearSetSlot.Ring1 or GearSetSlot.Ring2, false) => CommonLoc.GearSlot_Ring,
        (GearSetSlot.SoulCrystal, _)                    => CommonLoc.GearSlot_SoulCrystal,
        _                                               => CommonLoc.undefined,
    };

    public static string FriendlyName(this ItemSource source) => source switch
    {
        ItemSource.Raid         => CommonLoc.ItemSource_Raid,
        ItemSource.Dungeon      => CommonLoc.ItemSource_Dungeon,
        ItemSource.Trial        => CommonLoc.GearSource_Trial,
        ItemSource.Tome         => CommonLoc.GearSource_Tome,
        ItemSource.Crafted      => CommonLoc.ItemSource_Crafted,
        ItemSource.AllianceRaid => CommonLoc.GearSource_Alliance,
        ItemSource.Quest        => CommonLoc.ItemSource_Quest,
        ItemSource.Relic        => CommonLoc.ItemSource_Relic,
        ItemSource.Shop         => CommonLoc.ItemSource_Shop,
        ItemSource.Loot         => CommonLoc.ItemSource_Loot,
        _                       => CommonLoc.undefined,
    };

    public static string FriendlyName(this InstanceType source) => source switch
    {
        InstanceType.Raid         => CommonLoc.ItemSource_Raid,
        InstanceType.Trial        => CommonLoc.GearSource_Trial,
        InstanceType.Dungeon      => CommonLoc.ItemSource_Dungeon,
        InstanceType.SoloInstance => CommonLoc.GroupType_Solo,
        _                         => CommonLoc.undefined,
    };

    public static string FriendlyName(this Role role) => role switch
    {
        Role.Tank   => CommonLoc.Role_Tank,
        Role.Healer => CommonLoc.Role_Healer,
        Role.Melee  => CommonLoc.Role_Melee,
        Role.Ranged => CommonLoc.Role_Ranged,
        Role.Caster => CommonLoc.Role_Caster,
        _           => CommonLoc.undefined,
    };

    public static string Name(this Month month) => month switch
    {

        Month.January   => CommonLoc.Month_January,
        Month.February  => CommonLoc.Month_February,
        Month.March     => CommonLoc.Month_March,
        Month.April     => CommonLoc.Month_April,
        Month.May       => CommonLoc.Month_May,
        Month.June      => CommonLoc.Month_June,
        Month.July      => CommonLoc.Month_July,
        Month.August    => CommonLoc.Month_August,
        Month.September => CommonLoc.Month_September,
        Month.October   => CommonLoc.Month_October,
        Month.November  => CommonLoc.Month_November,
        Month.December  => CommonLoc.Month_December,
        _               => CommonLoc.Unknown,
    };
    public static string Abrrev(this Month month) => month switch
    {

        Month.January   => CommonLoc.Month_Abrrev_January,
        Month.February  => CommonLoc.Month_Abrrev_February,
        Month.March     => CommonLoc.Month_Abrrev_March,
        Month.April     => CommonLoc.Month_Abrrev_April,
        Month.May       => CommonLoc.Month_Abrrev_May,
        Month.June      => CommonLoc.Month_Abrrev_June,
        Month.July      => CommonLoc.Month_Abrrev_July,
        Month.August    => CommonLoc.Month_Abrrev_August,
        Month.September => CommonLoc.Month_Abrrev_September,
        Month.October   => CommonLoc.Month_Abrrev_October,
        Month.November  => CommonLoc.Month_Abrrev_November,
        Month.December  => CommonLoc.Month_Abrrev_December,
        _               => throw new ArgumentOutOfRangeException(nameof(month), month, null),
    };

    public static string Name(this Weekday day) => day switch
    {

        Weekday.Monday    => CommonLoc.Weekday_Monday,
        Weekday.Tuesday   => CommonLoc.Weekday_Tuesday,
        Weekday.Wednesday => CommonLoc.Weekday_Wednesday,
        Weekday.Thursday  => CommonLoc.Weekday_Thursday,
        Weekday.Friday    => CommonLoc.Weekday_Friday,
        Weekday.Saturday  => CommonLoc.Weekday_Saturday,
        Weekday.Sunday    => CommonLoc.Weekday_Sunday,
        _                 => throw new ArgumentOutOfRangeException(nameof(day), day, null),
    };

    public static string Abbrev(this Weekday day) => day switch
    {

        Weekday.Monday    => CommonLoc.Weekday_Abbrev_Monday,
        Weekday.Tuesday   => CommonLoc.Weekday_Abbrev_Tuesday,
        Weekday.Wednesday => CommonLoc.Weekday_Abbrev_Wednesday,
        Weekday.Thursday  => CommonLoc.Weekday_Abbrev_Thursday,
        Weekday.Friday    => CommonLoc.Weekday_Abbrev_Friday,
        Weekday.Saturday  => CommonLoc.Weekday_Abbrev_Saturday,
        Weekday.Sunday    => CommonLoc.Weekday_Abbrev_Sunday,
        _                 => throw new ArgumentOutOfRangeException(nameof(day), day, null),
    };
}