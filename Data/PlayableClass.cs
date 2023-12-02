using System;
using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XIVCalc.Calculations;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class PlayableClass
{
    private static readonly ExcelSheet<ClassJob>? _classJobSheet = ServiceManager.ExcelModule.GetSheet<ClassJob>();
    [JsonProperty("Job")]
    public Job Job;
    [JsonIgnore]
    public ClassJob ClassJob => _classJobSheet?.GetRow((uint)Job)!;
    public Role Role => Job.GetRole();
    public Character? Parent { get; private set; }
    [JsonProperty("Level")]
    public int Level = 1;
    [JsonProperty("Gear")]
    public GearSet Gear;
    [JsonProperty("BIS")]
    public GearSet Bis;
    [JsonConstructor]
    private PlayableClass()
    {
        Gear = new GearSet();
        Bis = new GearSet();
    }
    public PlayableClass(Job job, Character c)
    {
        Job = job;
        Gear = new GearSet(GearSetManager.Hrt);
        Bis = new GearSet(GearSetManager.Hrt, "BIS");
        Parent = c;
    }
    public (GearItem, GearItem) this[GearSetSlot slot]
    {
        get
        {
            GearSetSlot slot2 = slot;
            if (slot is not (GearSetSlot.Ring1 or GearSetSlot.Ring2))
                return (Gear[slot], Bis[slot2]);
            if (Gear[GearSetSlot.Ring2].Equals(Bis[GearSetSlot.Ring1], ItemComparisonMode.IdOnly)
                || Gear[GearSetSlot.Ring1].Equals(Bis[GearSetSlot.Ring2], ItemComparisonMode.IdOnly))
                slot2 = slot == GearSetSlot.Ring1 ? GearSetSlot.Ring2 : GearSetSlot.Ring1;
            return (Gear[slot], Bis[slot2]);
        }
    }
    public IEnumerable<(GearSetSlot, (GearItem, GearItem))> ItemTuples => GearSet.Slots.Select(slot => (slot, this[slot]));
    /// <summary>
    /// Evaluates if all of the given slots have BiS item or an item with higher or euqla item level as given item
    /// </summary>
    /// <param name="slots">List of slots to evaluate</param>
    /// <param name="toCompare">Item to compare to items in slots</param>
    /// <returns>True if all slots are BiS or better</returns>
    public bool HaveBisOrHigherItemLevel(IEnumerable<GearSetSlot> slots, GearItem toCompare) => SwappedCompare((item, bis) => BisOrBetterComparer(item, bis, toCompare), slots);
    /// <summary>
    /// Evaluates if all given slots already are equipped with Best in Slot
    /// </summary>
    /// <param name="slots">List of slots to check</param>
    /// <returns>True if all slots have BiS</returns>
    public bool HaveBis(IEnumerable<GearSetSlot> slots) => SwappedCompare(BisComparer, slots);
    private bool SwappedCompare(Func<GearItem, GearItem, bool> comparer, IEnumerable<GearSetSlot> slots)
    {
        var gearSetSlots = slots as GearSetSlot[] ?? slots.ToArray();
        if (gearSetSlots.Contains(GearSetSlot.Ring1) && gearSetSlots.Contains(GearSetSlot.Ring2))
            return (
                       SwappedCompare(comparer, GearSetSlot.Ring1, true, false) && SwappedCompare(comparer, GearSetSlot.Ring2, true, false)
                       || SwappedCompare(comparer, GearSetSlot.Ring1, true, true) && SwappedCompare(comparer, GearSetSlot.Ring2, true, true))
                   && gearSetSlots.Where(slot => slot is not (GearSetSlot.Ring1 or GearSetSlot.Ring2)).All(slot => SwappedCompare(comparer, slot));
        return gearSetSlots.All(slot => SwappedCompare(comparer, slot));
    }
    private bool SwappedCompare(Func<GearItem, GearItem, bool> comparer, GearSetSlot slot, bool explicitSwaps = false, bool ringsSwapped = false)
    {
        if (slot != GearSetSlot.Ring1 && slot != GearSetSlot.Ring2)
            return comparer(Gear[slot], Bis[slot]);
        return explicitSwaps switch
        {
            true when !ringsSwapped => comparer(Gear[slot], Bis[slot]),
            true when slot == GearSetSlot.Ring1 => comparer(Gear[slot], Bis[GearSetSlot.Ring2]),
            true => comparer(Gear[slot], Bis[GearSetSlot.Ring1]),
            false => comparer(Gear[slot], Bis[slot]) || slot == GearSetSlot.Ring1 && comparer(Gear[slot], Bis[GearSetSlot.Ring2]) || slot == GearSetSlot.Ring2 && comparer(Gear[slot], Bis[GearSetSlot.Ring1]),
        };
    }
    private static bool BisOrBetterComparer(GearItem item, GearItem bis, GearItem comp) => BisComparer(item, bis) || HigherILvlComparer(item, comp);
    private static bool HigherILvlComparer(GearItem item, GearItem comp) => item.ItemLevel >= comp.ItemLevel;
    private static bool BisComparer(GearItem item, GearItem bis) => item.Id == bis.Id;
    public int GetCurrentStat(StatType type) => GetStat(type, Gear);
    public int GetBiSStat(StatType type) => GetStat(type, Bis);
    public int GetStat(StatType type, IReadOnlyGearSet set)
    {
        type = type switch
        {
            StatType.AttackMagicPotency => Job.MainStat(),
            StatType.HealingMagicPotency => StatType.Mind,
            StatType.AttackPower => Job.MainStat(),
            _ => type,
        };
        return set.GetStat(type) //Gear Stats
               + (int)Math.Round(LevelTable.GetBaseStat((byte)type, Level) * StatEquations.GetJobModifier((byte)type, ClassJob)) //Base Stat dependent on job
               + (Parent?.Tribe?.GetRacialModifier(type) ?? 0); //"Racial" modifier +- up to 2
    }
    public void SetParent(Character c) => Parent = c;
    public bool IsEmpty => Level == 1 && Gear.IsEmpty && Bis.IsEmpty;

    public bool Equals(PlayableClass? other)
    {
        if (other == null)
            return false;
        return Job == other.Job;
    }
    public override string ToString() => $"{Job} ({Level})";
}