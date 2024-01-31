using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

public class GameExpansion
{
    [JsonProperty] public readonly byte GameVersion;
    [JsonProperty] public readonly MateriaLevel MaxMateriaLevel;
    [JsonProperty] public readonly int MaxLevel;
    public RaidTier[] SavageRaidTiers { get; init; }
    public RaidTier[] NormalRaidTiers { get; init; }

    public RaidTier? CurrentSavage => SavageRaidTiers.Any() ? SavageRaidTiers[^1] : null;

    public GameExpansion(byte v, MateriaLevel maxMatLevel, int maxLvl, int unlockedRaidTiers = 0)
    {
        GameVersion = v;
        MaxMateriaLevel = maxMatLevel;
        MaxLevel = maxLvl;
        NormalRaidTiers = new RaidTier[unlockedRaidTiers];
        SavageRaidTiers = new RaidTier[unlockedRaidTiers];
    }
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
    public readonly EncounterDifficulty Difficulty;
    public readonly uint WeaponItemLevel;
    public readonly uint ArmorItemLevel;
    public readonly string Name;
    private readonly List<uint> _bossIDs;
    public List<InstanceWithLoot> Bosses => _bossIDs.ConvertAll(id => ServiceManager.GameInfo.GetInstance(id));

    public RaidTier(EncounterDifficulty difficulty, uint weaponItemLevel, uint armorItemLevel, string name,
        IEnumerable<uint> bossIds)
    {
        Difficulty = difficulty;
        WeaponItemLevel = weaponItemLevel;
        ArmorItemLevel = armorItemLevel;
        Name = name;
        _bossIDs = new List<uint>(bossIds);
    }

    public uint ItemLevel(GearSetSlot slot) => slot == GearSetSlot.MainHand ? WeaponItemLevel : ArmorItemLevel;
}