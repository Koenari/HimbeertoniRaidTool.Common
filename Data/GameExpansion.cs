using HimbeertoniRaidTool.Common.Localization;

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

    public RaidTier? CurrentSavage => SavageRaidTiers.Length != 0 ? SavageRaidTiers[^1] : null;

    internal RaidTier? PreviousSavage => SavageRaidTiers.Length > 1 ? SavageRaidTiers[^2] : null;

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

public class RaidTier(
    EncounterDifficulty difficulty,
    int weaponItemLevel,
    int armorItemLevel,
    string name,
    IEnumerable<uint> bossIds)
{
    public static readonly RaidTier Empty = new(EncounterDifficulty.None, 0, 0, string.Empty, []);
    private readonly List<uint> _bossIDs = [..bossIds];
    public readonly int ArmorItemLevel = armorItemLevel;
    public readonly EncounterDifficulty Difficulty = difficulty;
    public readonly string Name = name;
    public readonly int WeaponItemLevel = weaponItemLevel;

    public List<InstanceWithLoot> Bosses => _bossIDs.ConvertAll(GameInfo.GetInstance);

    public int ItemLevel(GearSetSlot slot) => slot == GearSetSlot.MainHand ? WeaponItemLevel : ArmorItemLevel;
}