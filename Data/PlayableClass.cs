using HimbeertoniRaidTool.Common.Data.Dto;
using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using XIVCalc.Calculations;
using XIVCalc.Interfaces;
using XIVCalc.Lumina;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class PlayableClass(Job job)
    : IHrtDataType, ICloneable<PlayableClass>, IConvertibleToDto<ClassDto>
{
    #region Static

    private static readonly ExcelSheet<ClassJob> ClassJobSheet = CommonLibrary.ExcelModule.GetSheet<ClassJob>();

    #endregion

    #region Serialized

    [JsonProperty("BisSets")] private readonly List<Reference<GearSet>> _bis = [];

    [JsonProperty("GearSets")] private readonly List<Reference<GearSet>> _gearSets = [];

    [JsonProperty("ActiveBiSIdx")] private int _curBisIdx;

    [JsonProperty("ActiveGearIndex")] private int _curGearIdx;

    [JsonProperty("Hide")] public bool HideInUi;

    [JsonProperty("Job")] public Job Job = job;

    [JsonProperty("Level")] public int Level = 1;

    #endregion

    public ClassJob ClassJob => ClassJobSheet.GetRow((uint)Job);

    public Role Role => Job.GetRole();

    public IEnumerable<GearSet> GearSets => _gearSets.Select(set => set.Data);
    public GearSet CurGear
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            //I HATE JSON
            _gearSets.RemoveAll(s => s is null);
            if (_gearSets.Count == 0)
            {
                var toAdd = new GearSet(GearSetManager.Hrt, "Current");
                toAdd.MarkAsSystemManaged();
                _gearSets.Add(toAdd);
            }
            _curGearIdx = Math.Clamp(_curGearIdx, 0, _gearSets.Count - 1);
            return _gearSets[_curGearIdx].Data;
        }
        set
        {
            if (_gearSets.Count > 0 && value.Equals(_gearSets[_curGearIdx].Data)) return;
            _curGearIdx = _gearSets.FindIndex(s => s.Data.Equals(value));
            if (_curGearIdx >= 0) return;
            _gearSets.Add(value);
            _curGearIdx = _gearSets.Count - 1;
            _curGearStatsCache = null;
        }
    }
    private IStatEquations? _curGearStatsCache;
    public IStatEquations CurGearStats =>
        _curGearStatsCache ??= new StatBlockEquations(new GearSetStatBlock(this, CurGear));

    public GearSet AutoUpdatedGearSet
    {
        get
        {
            if (_gearSets.Any(set => set.Data.IsSystemManaged))
                return _gearSets.First(set => set.Data.IsSystemManaged).Data;
            var sysManaged = new GearSet(GearSetManager.Hrt, "Current");
            sysManaged.MarkAsSystemManaged();
            _gearSets.Add(sysManaged);
            return sysManaged;
        }
    }

    public IEnumerable<GearSet> BisSets => _bis.Select(reference => reference.Data);

    public GearSet CurBis
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            //I HATE JSON
            _bis.RemoveAll(s => s is null);
            if (_bis.Count == 0)
                _bis.Add(new GearSet(GearSetManager.Hrt, "BiS"));
            _curBisIdx = Math.Clamp(_curBisIdx, 0, _bis.Count - 1);
            return _bis[_curBisIdx].Data;
        }
        set
        {
            if (_bis.Count > 0 && value.Equals(_bis[_curBisIdx].Data)) return;
            _curBisIdx = _bis.FindIndex(s => s.Data.Equals(value));
            if (_curBisIdx >= 0) return;
            _bis.Add(value);
            _curBisIdx = _bis.Count - 1;
            _curBisStatsCache = null;
        }
    }

    private IStatEquations? _curBisStatsCache;
    public IStatEquations CurBisStats =>
        _curBisStatsCache ??= new StatBlockEquations(new GearSetStatBlock(this, CurBis));

    public IJobStatBlock CurBisStatBlock => new GearSetStatBlock(this, CurBis);
    public (GearItem, GearItem) this[GearSetSlot slot]
    {
        get
        {
            var slot2 = slot;
            if (slot is not (GearSetSlot.Ring1 or GearSetSlot.Ring2))
                return (CurGear[slot], CurBis[slot2]);
            if (CurGear[GearSetSlot.Ring2].Equals(CurBis[GearSetSlot.Ring1], ItemComparisonMode.IdOnly)
             || CurGear[GearSetSlot.Ring1].Equals(CurBis[GearSetSlot.Ring2], ItemComparisonMode.IdOnly))
                slot2 = slot == GearSetSlot.Ring1 ? GearSetSlot.Ring2 : GearSetSlot.Ring1;
            return (CurGear[slot], CurBis[slot2]);
        }
    }
    public IEnumerable<(GearSetSlot, (GearItem, GearItem))> ItemTuples =>
        GearSet.Slots.Select(slot => (slot, this[slot]));
    public bool IsEmpty => Level == 1 && CurGear.IsEmpty && CurBis.IsEmpty;
    public static string DataTypeName => CommonLoc.DataTypeName_Job;
    public string Name => Job.ToString();
    /// <summary>
    ///     Evaluates if all the given slots have BiS item or an item with higher or equal item level as
    ///     given item
    /// </summary>
    /// <param name="slots">List of slots to evaluate</param>
    /// <param name="toCompare">LuminaItem to compare to items in slots</param>
    /// <returns>True if all slots are BiS or better</returns>
    public bool HaveBisOrHigherItemLevel(IEnumerable<GearSetSlot> slots, GearItem toCompare) =>
        SwappedCompare((item, bis) => BisOrBetterComparer(item, bis, toCompare), slots);
    /// <summary>
    ///     Evaluates if all given slots already are equipped with Best in Slot
    /// </summary>
    /// <param name="slots">List of slots to check</param>
    /// <returns>True if all slots have BiS</returns>
    public bool HaveBis(IEnumerable<GearSetSlot> slots) => SwappedCompare(BisComparer, slots);
    private bool SwappedCompare(Func<GearItem, GearItem, bool> comparer, IEnumerable<GearSetSlot> slots)
    {
        var gearSetSlots = slots as GearSetSlot[] ?? slots.ToArray();
        if (gearSetSlots.Contains(GearSetSlot.Ring1) && gearSetSlots.Contains(GearSetSlot.Ring2))
            return (
                       SwappedCompare(comparer, GearSetSlot.Ring1, true)
                    && SwappedCompare(comparer, GearSetSlot.Ring2, true)
                    || SwappedCompare(comparer, GearSetSlot.Ring1, true, true)
                    && SwappedCompare(comparer, GearSetSlot.Ring2, true, true))
                && gearSetSlots.Where(slot => slot is not (GearSetSlot.Ring1 or GearSetSlot.Ring2))
                               .All(slot => SwappedCompare(comparer, slot));
        return gearSetSlots.All(slot => SwappedCompare(comparer, slot));
    }
    private bool SwappedCompare(Func<GearItem, GearItem, bool> comparer, GearSetSlot slot, bool explicitSwaps = false,
                                bool ringsSwapped = false)
    {
        if (slot != GearSetSlot.Ring1 && slot != GearSetSlot.Ring2)
            return comparer(CurGear[slot], CurBis[slot]);
        return explicitSwaps switch
        {
            true when !ringsSwapped             => comparer(CurGear[slot], CurBis[slot]),
            true when slot == GearSetSlot.Ring1 => comparer(CurGear[slot], CurBis[GearSetSlot.Ring2]),
            true                                => comparer(CurGear[slot], CurBis[GearSetSlot.Ring1]),
            false => comparer(CurGear[slot], CurBis[slot])
                  || slot == GearSetSlot.Ring1 && comparer(CurGear[slot], CurBis[GearSetSlot.Ring2])
                  || slot == GearSetSlot.Ring2 && comparer(CurGear[slot], CurBis[GearSetSlot.Ring1]),
        };
    }
    private static bool BisOrBetterComparer(GearItem item, GearItem bis, GearItem comp) =>
        BisComparer(item, bis) || HigherILvlComparer(item, comp);
    private static bool HigherILvlComparer(GearItem item, GearItem comp) => item.ItemLevel >= comp.ItemLevel;
    private static bool BisComparer(GearItem item, GearItem bis) => item.Id == bis.Id;
    public int GetCurrentStat(StatType type, Tribe? tribe) => GetStat(type, CurGear, tribe);
    public int GetBiSStat(StatType type, Tribe? tribe) => GetStat(type, CurBis, tribe);
    public int GetStat(StatType type, IReadOnlyGearSet set, Tribe? tribe)
    {
        type = type switch
        {
            StatType.AttackMagicPotency  => Job.MainStat(),
            StatType.HealingMagicPotency => StatType.Mind,
            StatType.AttackPower         => Job.MainStat(),
            _                            => type,
        };
        int statBase = 0;
        if (type is >= StatType.Strength and <= StatType.Mind)
            statBase = LevelTable.IntMAIN(Level);
        if (type is StatType.Piety or StatType.Tenacity or StatType.CriticalHit or StatType.Determination
                 or StatType.SkillSpeed or StatType.SpellSpeed or StatType.DirectHitRate)
            statBase = LevelTable.IntSUB(Level);
        int preFood = set.GetStat(type) //Gear Stats
                    + (int)Math.Floor(
                          statBase * StatEquations.GetJobModifier(
                              type, new LuminaJobModifiers(ClassJob))) //Base Stat dependent on job
                    + (tribe?.GetRacialModifier(type) ?? 0); //"Racial" modifier +- up to 2

        return set.Food?.ApplyEffect(type, preFood) ?? preFood;
    }

    public PlayableClass Clone() => CloneService.Clone(this);
    public bool Equals(PlayableClass? other)
    {
        if (other == null)
            return false;
        return Job == other.Job;
    }
    public override string ToString() => $"{Job} ({Level})";
    public void RemoveEmptySets()
    {
        _gearSets.RemoveAll(set => set.Data is { IsEmpty: true, LocalId.IsEmpty: true });
        _bis.RemoveAll(set => set.Data is { IsEmpty: true, LocalId.IsEmpty: true });
    }

    public void RemoveGearSet(GearSet gearSet) => _gearSets.Remove(gearSet);

    public void RemoveBisSet(GearSet bisSet) => _bis.Remove(bisSet);
    public ClassDto ToDto() => new(this);
    public void UpdateFromDto(ClassDto dto) => throw new NotImplementedException();
}

public class GearSetStatBlock(
    PlayableClass jobClass,
    IReadOnlyGearSet set,
    Tribe? tribe = null,
    PartyBonus bonus = PartyBonus.None) : IJobStatBlock
{
    public Job Job => jobClass.Job;
    public IStatEquations StatEquations => new StatBlockEquations(this);
    public IJobModifiers JobModifiers => new LuminaJobModifiers(jobClass.ClassJob);
    public int Level => jobClass.Level;
    public int WeaponDamage => set[GearSetSlot.MainHand].GetStat(StatType.PhysicalDamage);
    public int WeaponDelay => set[GearSetSlot.MainHand].GetStat(StatType.Delay);
    public int Vitality =>
        (int)XIVCalc.Calculations.StatEquations.MainStatWithPartyBonus(
            bonus, jobClass.GetStat(StatType.Vitality, set, tribe));
    public int Strength =>
        (int)XIVCalc.Calculations.StatEquations.MainStatWithPartyBonus(
            bonus, jobClass.GetStat(StatType.Strength, set, tribe));
    public int Dexterity =>
        (int)XIVCalc.Calculations.StatEquations.MainStatWithPartyBonus(
            bonus, jobClass.GetStat(StatType.Dexterity, set, tribe));
    public int Intelligence =>
        (int)XIVCalc.Calculations.StatEquations.MainStatWithPartyBonus(
            bonus, jobClass.GetStat(StatType.Intelligence, set, tribe));
    public int Mind =>
        (int)XIVCalc.Calculations.StatEquations.MainStatWithPartyBonus(
            bonus, jobClass.GetStat(StatType.Mind, set, tribe));
    public int PhysicalDefense => jobClass.GetStat(StatType.Defense, set, tribe);
    public int MagicalDefense => jobClass.GetStat(StatType.MagicDefense, set, tribe);
    public int AttackPower => jobClass.GetStat(StatType.AttackPower, set, tribe);
    public int AttackMagicPotency => jobClass.GetStat(StatType.AttackMagicPotency, set, tribe);
    public int HealingMagicPotency => jobClass.GetStat(StatType.HealingMagicPotency, set, tribe);
    public int DirectHit => jobClass.GetStat(StatType.DirectHitRate, set, tribe);
    public int CriticalHit => jobClass.GetStat(StatType.CriticalHit, set, tribe);
    public int Determination => jobClass.GetStat(StatType.Determination, set, tribe);
    public int SkillSpeed => jobClass.GetStat(StatType.SkillSpeed, set, tribe);
    public int SpellSpeed => jobClass.GetStat(StatType.SpellSpeed, set, tribe);
    public int Piety => jobClass.GetStat(StatType.Piety, set, tribe);
    public int Tenacity => jobClass.GetStat(StatType.Tenacity, set, tribe);
}