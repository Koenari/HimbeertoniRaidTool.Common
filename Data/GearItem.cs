using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[method: JsonConstructor]
public class GearItem(uint id = 0, bool hq = false) : HqItem(id, hq), ICloneable<GearItem>
{
    public static readonly new GearItem Empty = new();

    #region Serialized

    [JsonProperty("Materia")] private readonly List<MateriaItem> _materia = [];

    [JsonProperty("RelicParams", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<StatType, int>? RelicStats;

    #endregion

    //This holds the total stats of this gear item (including materia)
    private readonly Dictionary<StatType, int> _statCache = new();

    public new string DataTypeName => CommonLoc.DataTypeName_item_gear;

    public IEnumerable<GearSetSlot> Slots => EquipSlotCategory.AvailableSlots();

    public IEnumerable<MateriaItem> Materia => _materia;

    public int MaxMateriaSlots => GameItem.IsAdvancedMeldingPermitted ? 5 : GameItem.MateriaSlotCount;

    public int StatCap => CalcStatCap();

    public IEnumerable<StatType> StatTypesAffected
    {
        get
        {
            SortedSet<StatType> done = new(GameItem.StatTypesAffected);
            if (IsRelic() && RelicStats is not null)
            {
                foreach ((var type, int value) in RelicStats)
                {
                    if (value > 0)
                        done.Add(type);
                }
            }
            foreach (var mat in Materia)
            {
                done.Add(mat.StatType);
            }
            return done;
        }
    }

    public new GearItem Clone() => CloneService.Clone(this);
    public bool Equals(GearItem? other) => Equals(other, ItemComparisonMode.Full);

    private void InvalidateCache() => _statCache.Clear();

    private int CalcStatCap()
    {
        //ToDo: Do actual Cap
        if (GameItem.Rarity == Rarity.Relic) return int.MaxValue;
        var secondaryStats = StatTypesAffected.Where(type => type.IsSecondary()).ToList();
        if (secondaryStats.Count == 0)
            return int.MaxValue;
        return secondaryStats.Select(stat => GameItem.GetStat(stat, IsHq)).Max();
    }

    public int GetStat(StatType type, bool includeMateria = true)
    {
        if (includeMateria && _statCache.TryGetValue(type, out int cached))
            return cached;
        int result = GameItem.GetStat(type, IsHq);
        if (IsRelic() && (RelicStats?.TryGetValue(type, out int val) ?? false))
            result += val;
        if (!includeMateria)
            return result;
        result += _materia.Where(x => x.StatType == type).Sum(materia => materia.GetStat());
        if (type.IsSecondary())
            result = int.Min(result, StatCap);
        _statCache.TryAdd(type, result);
        return result;
    }

    public bool Equals(GearItem? other, ItemComparisonMode mode)
    {
        //idOnly
        if (Id != other?.Id) return false;
        if (mode == ItemComparisonMode.IdOnly) return true;
        //IgnoreMateria
        if (IsHq != other.IsHq) return false;
        if (mode == ItemComparisonMode.IgnoreMateria) return true;
        //Full
        Dictionary<StatType, int> matVals = new();
        foreach (var s in _materia)
        {
            if (!matVals.TryAdd(s.StatType, s.GetStat()))
                matVals[s.StatType] += s.GetStat();
        }
        foreach (var s in other._materia)
        {
            if (!matVals.ContainsKey(s.StatType))
                return false;
            matVals[s.StatType] -= s.GetStat();

        }
        return matVals.Values.All(s => s == 0);
    }
    public bool IsRelic() => GameItem.Rarity == Rarity.Relic;
    public bool CanAffixMateria() => _materia.Count < MaxMateriaSlots;

    public void AddMateria(MateriaItem materiaItem)
    {
        if (!CanAffixMateria()) return;
        _materia.Add(materiaItem);
        InvalidateCache();
    }

    public void RemoveMateria(int removeAt)
    {
        if (removeAt >= _materia.Count) return;
        _materia.RemoveAt(removeAt);
        InvalidateCache();
    }

    public void ReplaceMateria(int index, MateriaItem newMat)
    {
        if (index >= _materia.Count) return;
        _materia[index] = newMat;
        InvalidateCache();
    }

    //ToDo: Do from Lumina
    public MateriaLevel MaxAffixableMateriaLevel()
    {
        if (!CanAffixMateria()) return 0;
        var maxAllowed = GameInfo.GetExpansionByLevel(GameItem.LevelEquip).MaxMateriaLevel;
        if (_materia.Count >= GameItem.MateriaSlotCount)
            maxAllowed--;

        return maxAllowed;
    }
}