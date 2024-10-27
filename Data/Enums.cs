using System.Diagnostics.CodeAnalysis;

namespace HimbeertoniRaidTool.Common.Data;

public enum Month : int
{
    January = 1,
    February = 2,
    March = 3,
    April = 4,
    May = 5,
    June = 6,
    July = 7,
    August = 8,
    September = 9,
    October = 10,
    November = 11,
    December = 12,
}

public enum Weekday
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}

public enum InviteStatus
{
    NotInvited = 0,
    Invited = 1,
    Declined = 2,
    Unsure = 3,
    Accepted = 4,
    Revoked = 5,
    Confirmed = 6,
    NoStatus = -1,
}

public enum EventStatus
{
    Unknown = 0,
    Scheduled = 1,
    Canceled = 2,
    Active = 3,
    Finished = 4,
}

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
    BisOverUpgrade = 1,
    LowestItemLevel = 2,
    HighestItemLevelGain = 3,
    RolePrio = 4,
    Random = 5,
    [Obsolete("DPSGain")] Dps = 6,
    DpsGain = 7,
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
    Hrt,
    Etro,
    XivGear,
}

public enum Gender
{
    Unknown = 0,
    Female = 1,
    Male = 2,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
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
    XI = 10,
    XII = 11,
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
    Gp = 20,
    Craftsmanship = 21,
    Cp = 22,
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