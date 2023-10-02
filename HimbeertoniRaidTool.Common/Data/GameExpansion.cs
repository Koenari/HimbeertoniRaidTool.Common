using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Services;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

public class GameExpansion
{
    [JsonProperty] public readonly byte GameVersion;
    [JsonProperty] public readonly MateriaLevel MaxMateriaLevel;
    [JsonProperty] public readonly int MaxLevel;
    public readonly RaidTier[] SavageRaidTiers;
    public readonly RaidTier[] NormalRaidTiers;

    public RaidTier CurrentSavage => SavageRaidTiers[^1];

    public GameExpansion(byte v, MateriaLevel maxMatLevel, int maxLvl, int unlockedRaidTiers)
    {
        GameVersion = v;
        MaxMateriaLevel = maxMatLevel;
        MaxLevel = maxLvl;
        NormalRaidTiers = new RaidTier[unlockedRaidTiers];
        SavageRaidTiers = new RaidTier[unlockedRaidTiers];
    }
}

public class RaidTier
{
    public readonly EncounterDifficulty Difficulty;
    public readonly uint WeaponItemLevel;
    public readonly uint ArmorItemLevel;
    public readonly string Name;
    private readonly List<uint> BossIDs;
    public List<InstanceWithLoot> Bosses => BossIDs.ConvertAll(id => ServiceManager.GameInfo.GetInstance(id));

    public RaidTier(EncounterDifficulty difficulty, uint weaponItemLevel, uint armorItemLevel, string name,
        IEnumerable<uint> bossIDS)
    {
        Difficulty = difficulty;
        WeaponItemLevel = weaponItemLevel;
        ArmorItemLevel = armorItemLevel;
        Name = name;
        BossIDs = new List<uint>(bossIDS);
    }

    public uint ItemLevel(GearSetSlot slot)
    {
        return slot == GearSetSlot.MainHand ? WeaponItemLevel : ArmorItemLevel;
    }
}