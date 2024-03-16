using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

public class GameExpansion
{
    [JsonProperty] public readonly byte GameVersion;
    [JsonProperty] public readonly int MaxLevel;
    [JsonProperty] public readonly MateriaLevel MaxMateriaLevel;

    public GameExpansion(byte v, MateriaLevel maxMatLevel, int maxLvl, int unlockedRaidTiers = 0)
    {
        GameVersion = v;
        MaxMateriaLevel = maxMatLevel;
        MaxLevel = maxLvl;
        NormalRaidTiers = new RaidTier[unlockedRaidTiers];
        SavageRaidTiers = new RaidTier[unlockedRaidTiers];
    }
    public RaidTier[] SavageRaidTiers { get; init; }
    public RaidTier[] NormalRaidTiers { get; init; }

    public RaidTier? CurrentSavage => SavageRaidTiers.Any() ? SavageRaidTiers[^1] : null;
    public string Name => GameVersion switch
    {
        2 => CommonLoc.Expansion_ARR,
        3 => CommonLoc.Expansion_HW,
        4 => CommonLoc.Expansion_SB,
        5 => CommonLoc.Expansion_ShB,
        6 => CommonLoc.Expansion_EW,
        7 => CommonLoc.Expansion_DT,
        _ => CommonLoc.Unknown,
    };
}

public class RaidTier
{
    private readonly List<uint> _bossIDs;
    public readonly int ArmorItemLevel;
    public readonly EncounterDifficulty Difficulty;
    public readonly string Name;
    public readonly int WeaponItemLevel;

    public RaidTier(EncounterDifficulty difficulty, int weaponItemLevel, int armorItemLevel, string name,
                    IEnumerable<uint> bossIds)
    {
        Difficulty = difficulty;
        WeaponItemLevel = weaponItemLevel;
        ArmorItemLevel = armorItemLevel;
        Name = name;
        _bossIDs = new List<uint>(bossIds);
    }
    public List<InstanceWithLoot> Bosses => _bossIDs.ConvertAll(id => ServiceManager.GameInfo.GetInstance(id));

    public int ItemLevel(GearSetSlot slot) => slot == GearSetSlot.MainHand ? WeaponItemLevel : ArmorItemLevel;
}