using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace HimbeertoniRaidTool.Common.Data;

public enum ItemSource
{
    Undefined = 0,

    Raid = 1,
    Dungeon = 2,
    Trial = 3,
    AllianceRaid = 101,

    Tome = 10,
    Crafted = 20,
    Quest = 30,
    Relic = 40,
    Shop = 50,
    Loot = 60,
}

public enum GearSetSlot : short
{
    MainHand = 0,
    OffHand = 1,
    Head = 2,
    Body = 3,
    Hands = 4,
    Waist = 5,
    Legs = 6,
    Feet = 7,
    Ear = 8,
    Neck = 9,
    Wrist = 10,
    Ring1 = 11,
    Ring2 = 12,
    SoulCrystal = 13,
    None = 999,
}

public enum LootRuleEnum
{
    None = 0,
    BISOverUpgrade = 1,
    LowestItemLevel = 2,
    HighestItemLevelGain = 3,
    RolePrio = 4,
    Random = 5,
    [Obsolete("DPSGain")] DPS = 6,
    DPSGain = 7,
    CanUse = 8,
    CanBuy = 9,
    NeedGreed = 997,
    Greed = 998,
    Custom = 999,
}

public enum EncounterDifficulty
{
    None = 0,
    Normal,
    Hard,
    Extreme,
    Savage,
    Ultimate,
}

public enum Job : byte
{
    ADV = 0,

    //Old jobs
    GLA = 1,
    PGL = 2,
    MRD = 3,
    LNC = 4,
    ARC = 5,
    CNJ = 6,
    THM = 7,

    //Crafter
    CRP = 8,
    BSM = 9,
    ARM = 10,
    GSM = 11,
    LTW = 12,
    WVR = 13,
    ALC = 14,
    CUL = 15,

    //Gatherer
    MIN = 16,
    BTN = 17,
    FSH = 18,

    //End game Jobs
    PLD = 19,
    MNK = 20,
    WAR = 21,
    DRG = 22,
    BRD = 23,
    WHM = 24,
    BLM = 25,
    ACN = 26,
    SMN = 27,
    SCH = 28,
    ROG = 29,
    NIN = 30,
    MCH = 31,
    DRK = 32,
    AST = 33,
    SAM = 34,
    RDM = 35,
    BLU = 36,
    GNB = 37,
    DNC = 38,
    RPR = 39,
    SGE = 40,
    Count,
}

public enum Role : byte
{
    None = 0,
    Tank = 1,
    Healer = 4,
    Melee = 2,
    Ranged = 3,
    Caster = 5,
    DoL = 6,
    DoH = 7,
}

public enum InstanceType
{
    Unknown = 0,
    Raid = 1,
    Trial = 4,
    Dungeon = 2,
    SoloInstance = 7,
}

public enum GroupType
{
    Solo,
    Group,
    Raid,
}

public enum GearSetManager
{
    HRT,
    Etro,
}

public enum Gender
{
    Unknown = 0,
    Female = 1,
    Male = 2,
}

public enum MateriaLevel : byte
{
    None = 255,
    I = 0,
    II = 1,
    III = 2,
    IV = 3,
    V = 4,
    VI = 5,
    VII = 6,
    VIII = 7,
    IX = 8,
    X = 9,
}

public enum MateriaCategory : ushort
{
    None = 0,
    Piety = 7,
    DirectHit = 14,
    CriticalHit = 15,
    Determination = 16,
    Tenacity = 17,
    Gathering = 18,
    Perception = 19,
    GP = 20,
    Craftsmanship = 21,
    CP = 22,
    Control = 23,
    SkillSpeed = 24,
    SpellSpeed = 25,
}

public enum Rarity : byte
{
    None = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Relic = 4,
}

public enum StatType : uint
{
    None,
    Strength,
    Dexterity,
    Vitality,
    Intelligence,
    Mind,
    Piety,
    HP,
    MP,
    TP,
    GP,
    CP,
    PhysicalDamage,
    MagicalDamage,
    Delay,
    AdditionalEffect,
    AttackSpeed,
    BlockRate,
    BlockStrength,
    Tenacity,
    AttackPower,
    Defense,
    DirectHitRate,
    Evasion,
    MagicDefense,
    CriticalHitPower,
    CriticalHitResilience,
    CriticalHit,
    CriticalHitEvasion,
    SlashingResistance,
    PiercingResistance,
    BluntResistance,
    ProjectileResistance,
    AttackMagicPotency,
    HealingMagicPotency,
    EnhancementMagicPotency,
    EnfeeblingMagicPotency,
    FireResistance,
    IceResistance,
    WindResistance,
    EarthResistance,
    LightningResistance,
    WaterResistance,
    MagicResistance,
    Determination,
    SkillSpeed,
    SpellSpeed,
    Haste,
    Morale,
    Enmity,
    EnmityReduction,
    CarefulDesynthesis,
    EXPBonus,
    Regen,
    Refresh,
    MovementSpeed,
    Spikes,
    SlowResistance,
    PetrificationResistance,
    ParalysisResistance,
    SilenceResistance,
    BlindResistance,
    PoisonResistance,
    StunResistance,
    SleepResistance,
    BindResistance,
    HeavyResistance,
    DoomResistance,
    ReducedDurabilityLoss,
    IncreasedSpiritbondGain,
    Craftsmanship,
    Control,
    Gathering,
    Perception,
    Unknown73,
    Count,
}

public static class EnumExtensions
{
    private static ExcelSheet<ClassJob>? JobSheetCache = null;
    private static ExcelSheet<ClassJob>? JobSheet => JobSheetCache ??= ServiceManager.ExcelModule.GetSheet<ClassJob>();
    private static readonly Dictionary<Job, ClassJob?> JobCache = new();

    private static ClassJob? GetClassJob(Job j)
    {
        if (!JobCache.ContainsKey(j))
            JobCache[j] = null;
        return JobCache[j] ??= JobSheet?.GetRow((uint)j);
    }

    public static Role GetRole(this Job c)
    {
        ClassJob? cj = GetClassJob(c);
        if (cj is null)
            return Role.None;
        if (cj.PartyBonus > 4)
            return (Role)cj.PartyBonus;
        return (Role)cj.Role;
    }

    public static bool IsCombatRole(this Role role) => role is Role.Tank or Role.Healer or Role.Melee or Role.Caster or Role.Ranged;

    public static StatType MainStat(this Job job) => (StatType)(GetClassJob(job)?.PrimaryStat ?? 0);

    public static int GroupSize(this GroupType groupType)
    {
        return groupType switch
        {
            GroupType.Solo => 1,
            GroupType.Group => 4,
            GroupType.Raid => 8,
            _ => 0,
        };
    }

    public static bool CanHaveShield(this Job job) => job is Job.PLD or Job.THM or Job.GLA or Job.CNJ;

    public static bool IsCombatJob(this Job j) => !(Job.CRP <= j && j <= Job.FSH);

    public static bool IsDoH(this Job j) => Job.MIN <= j && j <= Job.FSH;

    public static bool IsDoL(this Job j) => Job.CRP <= j && j <= Job.CUL;

    public static StatType GetStatType(this MateriaCategory materiaCategory)
    {
        return materiaCategory switch
        {
            MateriaCategory.Piety => StatType.Piety,
            MateriaCategory.DirectHit => StatType.DirectHitRate,
            MateriaCategory.CriticalHit => StatType.CriticalHit,
            MateriaCategory.Determination => StatType.Determination,
            MateriaCategory.Tenacity => StatType.Tenacity,
            MateriaCategory.Gathering => StatType.Gathering,
            MateriaCategory.Perception => StatType.Perception,
            MateriaCategory.GP => StatType.GP,
            MateriaCategory.Craftsmanship => StatType.Craftsmanship,
            MateriaCategory.CP => StatType.CP,
            MateriaCategory.Control => StatType.Control,
            MateriaCategory.SkillSpeed => StatType.SkillSpeed,
            MateriaCategory.SpellSpeed => StatType.SpellSpeed,
            _ => StatType.None,
        };
    }

    public static ItemSource ToItemSource(this InstanceType contentType)
    {
        return contentType switch
        {
            InstanceType.Raid => ItemSource.Raid,
            InstanceType.Trial => ItemSource.Trial,
            InstanceType.Dungeon => ItemSource.Dungeon,
            _ => ItemSource.Undefined,
        };
    }

    public static bool IsSecondary(this StatType statType)
    {
        return statType switch
        {
            StatType.CriticalHit => true,
            StatType.Determination => true,
            StatType.DirectHitRate => true,
            StatType.SpellSpeed => true,
            StatType.SkillSpeed => true,
            StatType.Piety => true,
            StatType.Tenacity => true,
            StatType.Craftsmanship => true,
            StatType.Control => true,
            StatType.Gathering => true,
            StatType.Perception => true,
            _ => false,
        };
    }
}