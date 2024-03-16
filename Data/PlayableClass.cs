using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVCalc.Calculations;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class PlayableClass : IHrtDataType
{
    private static readonly ExcelSheet<ClassJob>? _classJobSheet = ServiceManager.ExcelModule.GetSheet<ClassJob>();

    [JsonProperty("BisSets")] private readonly List<GearSet> _bis = new();

    [JsonProperty("GearSets")] private readonly List<GearSet> _gearSets = new();

    [JsonProperty("ActiveBiSIdx")] private int _curBisIdx;
    [JsonProperty("ActiveGearIndex")] private int _curGearIdx;
    [JsonProperty("Hide")] public bool HideInUi;

    [JsonProperty("Job")]
    public Job Job;
    [JsonProperty("Level")] public int Level = 1;
    public PlayableClass(Job job)
    {
        Job = job;
    }
    [JsonIgnore]
    public ClassJob ClassJob => _classJobSheet?.GetRow((uint)Job)!;
    public Role Role => Job.GetRole();
    [JsonProperty("Gear")] [Obsolete("Use CurGear", true)]
    private GearSet GearMigration
    {
        set
        {
            value.MarkAsSystemManaged();
            if (value.Name.Equals("HrtCurrent"))
                value.Name = "Current";
            _gearSets.Insert(0, value);
        }
    }
    [JsonIgnore] public IEnumerable<GearSet> GearSets => _gearSets;
    [JsonIgnore] public GearSet CurGear
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
            return _gearSets[_curGearIdx];
        }
        set
        {
            if (_gearSets.Count > 0 && value.Equals(_gearSets[_curGearIdx])) return;
            _curGearIdx = _gearSets.FindIndex(s => s.Equals(value));
            if (_curGearIdx >= 0) return;
            _gearSets.Add(value);
            _curGearIdx = _gearSets.Count - 1;
        }
    }

    public GearSet AutoUpdatedGearSet
    {
        get
        {
            if (_gearSets.Any(set => set.IsSystemManaged))
                return _gearSets.First(set => set.IsSystemManaged);
            var sysManaged = new GearSet(GearSetManager.Hrt, "Current");
            sysManaged.MarkAsSystemManaged();
            _gearSets.Add(sysManaged);
            return sysManaged;
        }
    }

    [JsonProperty("BIS")] [Obsolete("Use CurBis", true)]
    private GearSet BisMigration { set => _bis.Insert(0, value); }

    [JsonIgnore] public IEnumerable<GearSet> BisSets => _bis;

    [JsonIgnore] public GearSet CurBis
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            //I HATE JSON
            _bis.RemoveAll(s => s is null);
            if (_bis.Count == 0)
                _bis.Add(new GearSet(GearSetManager.Hrt, "BiS"));
            _curBisIdx = Math.Clamp(_curBisIdx, 0, _bis.Count - 1);
            return _bis[_curBisIdx];
        }
        set
        {
            if (_bis.Count > 0 && value.Equals(_bis[_curBisIdx])) return;
            _curBisIdx = _bis.FindIndex(s => s.Equals(value));
            if (_curBisIdx >= 0) return;
            _bis.Add(value);
            _curBisIdx = _bis.Count - 1;
        }
    }
    public (GearItem, GearItem) this[GearSetSlot slot]
    {
        get
        {
            GearSetSlot slot2 = slot;
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
    [JsonIgnore] public static string DataTypeNameStatic => CommonLoc.DataTypeName_Job;
    [JsonIgnore] public string DataTypeName => DataTypeNameStatic;
    [JsonIgnore] public string Name => Job.ToString();
    /// <summary>
    ///     Evaluates if all of the given slots have BiS item or an item with higher or equal item level as
    ///     given item
    /// </summary>
    /// <param name="slots">List of slots to evaluate</param>
    /// <param name="toCompare">Item to compare to items in slots</param>
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
        return set.GetStat(type) //Gear Stats
             + (int)Math.Floor(LevelTable.GetBaseStat((byte)type, Level)
                             * StatEquations.GetJobModifier((byte)type, ClassJob)) //Base Stat dependent on job
             + (tribe?.GetRacialModifier(type) ?? 0); //"Racial" modifier +- up to 2
    }

    public bool Equals(PlayableClass? other)
    {
        if (other == null)
            return false;
        return Job == other.Job;
    }
    public override string ToString() => $"{Job} ({Level})";
    public void RemoveEmptySets()
    {
        _gearSets.RemoveAll(set => set is { IsEmpty: true, LocalId.IsEmpty: true });
        _bis.RemoveAll(set => set is { IsEmpty: true, LocalId.IsEmpty: true });
    }
}