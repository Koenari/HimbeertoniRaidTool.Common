using Lumina.Excel.GeneratedSheets;
using XIVCalc.Calculations;

namespace HimbeertoniRaidTool.Common.Calculations;

public static class AllaganLibrary
{
    /// <summary>
    /// Formats the mathematically correct value according to it's type
    /// </summary>
    /// <param name="type">Type of stat that should be evaluated</param>
    /// <param name="evaluatedValue">value to be formatted</param>
    /// <param name="alternative">a way to use alternative formulas for stats that have multiple effects (0 is default formula)</param>
    /// <returns>Evaluated value including unit</returns>
    public static (string Val, string Unit) FormatStatValue(double evaluatedValue, StatType type, int alternative = 0)
    {
        var notAvail = ("n.A.", "");
        if (double.IsNaN(evaluatedValue))
            return notAvail;
        return (type, alternative) switch
        {
            (StatType.CriticalHit, _)   => ($"{evaluatedValue * 100:N1}", "%%"),
            (StatType.DirectHitRate, _) => ($"{evaluatedValue * 100:N1}", "%%"),
            (StatType.Determination, _) => ($"{evaluatedValue * 100:N1}", "%%"),
            (StatType.Tenacity, _)      => ($"{evaluatedValue * 100:N1}", "%%"),
            (StatType.Piety, _)         => ($"{evaluatedValue:N0}", "MP/s"),
            //AA/DoT Multiplier
            (StatType.SkillSpeed, 1) or (StatType.SpellSpeed, 1) => ($"{evaluatedValue * 100:N2}", "%%"),
            //GCD
            (StatType.SkillSpeed, _) or (StatType.SpellSpeed, _)        => ($"{evaluatedValue:N2}", "s"),
            (StatType.Defense, _) or (StatType.MagicDefense, _)         => ($"{evaluatedValue * 100:N1}", "%%"),
            (StatType.Vitality, _)                                      => ($"{evaluatedValue:N0}", "HP"),
            (StatType.MagicalDamage, _) or (StatType.PhysicalDamage, _) => ($"{evaluatedValue * 100:N0}", "Dmg/100"),
            _                                                           => notAvail,
        };
    }
    /// <summary>
    /// This function evaluates a stat to it's respective effective used effect.
    /// Only works for level 90/80/70 (some stats may work for other levels too)
    /// </summary>
    /// <param name="type">Type of stat that should be evaluated</param>
    /// <param name="curClass">The job/class to evaluate for</param>
    /// <param name="gear">current gear set to get stats</param>
    /// <param name="tribe">Tribe of character</param>
    /// <param name="alternative">a way to use alternative formulas for stats that have multiple effects (0 is default formula)</param>
    /// <returns>Evaluated value (percentage values are in mathematical correct value, means 100% = 1.0)</returns>
    public static double EvaluateStat(StatType type, PlayableClass curClass, IReadOnlyGearSet gear, Tribe? tribe,
                                      int alternative = 0)
    {
        //ToDO: reenable when XIvCalc is updated
        return double.NaN;
        int totalStat = curClass.GetStat(type, gear, tribe);
        int level = curClass.Level;
        return (type, alternative) switch
        {
            (StatType.CriticalHit, 1)   => StatEquations.CalcCritDamage(totalStat, level),
            (StatType.CriticalHit, _)   => StatEquations.CalcCritRate(totalStat, level),
            (StatType.DirectHitRate, _) => StatEquations.CalcDirectHitRate(totalStat, level),
            (StatType.Determination, _) => StatEquations.CalcDeterminationMultiplier(totalStat, level),
            //Outgoing DMG
            (StatType.Tenacity, 1) => 1f + StatEquations.CalcTenacityModifier(totalStat, level),
            //Incoming DMG
            (StatType.Tenacity, _) => 1f - StatEquations.CalcTenacityModifier(totalStat, level),
            (StatType.Piety, _)    => StatEquations.CalcMPPerSecond(totalStat, level),
            //AA/DoT Multiplier
            (StatType.SkillSpeed or StatType.SpellSpeed, 1) => StatEquations.CalcAADotMultiplier(totalStat, level),
            //GCD
            (StatType.SkillSpeed or StatType.SpellSpeed, _) => StatEquations.CalcGCD(totalStat, level),
            (StatType.Defense or StatType.MagicDefense, _) => StatEquations.CalcDefenseMitigation(totalStat, level),
            (StatType.Vitality, _) => StatEquations.CalcHP(totalStat, level, curClass.ClassJob),
            (StatType.PhysicalDamage, _)
                => StatEquations.CalcAverageDamagePer100(
                    curClass.GetStat(StatType.PhysicalDamage, gear, tribe),
                    curClass.GetStat((StatType)curClass.ClassJob.PrimaryStat, gear, tribe),
                    curClass.GetStat(StatType.CriticalHit, gear, tribe),
                    curClass.GetStat(StatType.DirectHitRate, gear, tribe),
                    curClass.GetStat(StatType.Determination, gear, tribe),
                    curClass.GetStat(StatType.Tenacity, gear, tribe),
                    level, curClass.ClassJob) *
                2.5 / StatEquations.CalcGCD(curClass.GetStat(StatType.SkillSpeed, gear, tribe), level),
            (StatType.MagicalDamage, _)
                => StatEquations.CalcAverageDamagePer100(
                    curClass.GetStat(StatType.PhysicalDamage, gear, tribe),
                    curClass.GetStat((StatType)curClass.ClassJob.PrimaryStat, gear, tribe),
                    curClass.GetStat(StatType.CriticalHit, gear, tribe),
                    curClass.GetStat(StatType.DirectHitRate, gear, tribe),
                    curClass.GetStat(StatType.Determination, gear, tribe),
                    curClass.GetStat(StatType.Tenacity, gear, tribe),
                    level, curClass.ClassJob) *
                2.5 / StatEquations.CalcGCD(curClass.GetStat(StatType.SpellSpeed, gear, tribe), level),
            _ => float.NaN,
        };
    }
}