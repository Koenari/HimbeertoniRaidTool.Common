using System;
using HimbeertoniRaidTool.Common.Data;
using XIVCalc.Calculations;

namespace HimbeertoniRaidTool.Common.Calculations;

public static class AllaganLibrary
{
    /// <summary>
    /// This function evaluates a stat to it's respective effective used effect.
    /// Only works for level 90/80/70
    /// </summary>
    /// <param name="type">Type of stat that should be evaluated</param>
    /// <param name="curClass">The job/class to evaluate for</param>
    /// <param name="bis">If true evaluates for BiS gear else for current</param>
    /// <param name="alternative">a way to use alternative formulas for stats that have multiple effects (0 is default furmula)</param>
    /// <returns>Evaluated value including unit</returns>    
    public static string EvaluateStatToDisplay(StatType type, PlayableClass curClass, bool bis, int alternative = 0)
    {
        string notAvail = "n.A.";
        float evaluatedValue = EvaluateStat(type, curClass, bis, alternative);
        if (float.IsNaN(evaluatedValue))
            return notAvail;
        return (type, alternative) switch
        {
            (StatType.CriticalHit, _) => $"{evaluatedValue * 100:N1} %%",
            (StatType.DirectHitRate, _) => $"{evaluatedValue * 100:N1} %%",
            (StatType.Determination, _) => $"{evaluatedValue * 100:N1} %%",
            (StatType.Tenacity, _) => $"{evaluatedValue * 100:N1} %%",
            (StatType.Piety, _) => $"+{evaluatedValue:N0} MP/s",
            //AA/DoT Multiplier
            (StatType.SkillSpeed, 1) or (StatType.SpellSpeed, 1) => $"{evaluatedValue * 100:N2} %%",
            //GCD
            (StatType.SkillSpeed, _) or (StatType.SpellSpeed, _) => $"{evaluatedValue:N2} s",
            (StatType.Defense, _) or (StatType.MagicDefense, _) => $"{evaluatedValue * 100:N1} %%",
            (StatType.Vitality, _) => $"{evaluatedValue:N0} HP",
            (StatType.MagicalDamage, _) or (StatType.PhysicalDamage, _) => $"{evaluatedValue * 100:N0} Dmg/100",
            _ => notAvail
        };
    }
    /// <summary>
    /// This function evaluates a stat to it's respective effective used effect.
    /// Only works for level 90/80/70 (some stats may work for aother levels too)
    /// </summary>
    /// <param name="type">Type of stat that should be evaluated</param>
    /// <param name="curClass">The job/class to evaluate for</param>
    /// <param name="bis">If true evaluates for BiS gear else for current</param>
    /// <param name="alternative">a way to use alternative formulas for stats that have multiple effects (0 is default furmula)</param>
    /// /// <param name="additionalStats">pass any additional stats that are necessary to calculate given vlaue</param>
    /// <returns>Evaluated value (percentage values are in mathematical correct value, means 100% = 1.0)</returns>
    public static float EvaluateStat(StatType type, PlayableClass curClass, bool bis, int alternative = 0)
    {
        Func<StatType, int> getStat = bis ? curClass.GetBiSStat : curClass.GetCurrentStat;
        int totalStat = bis ? curClass.GetBiSStat(type) : curClass.GetCurrentStat(type);
        int level = curClass.Level;
        var job = curClass.Job;
        return (type, alternative) switch
        {
            (StatType.CriticalHit, 1) => StatEquations.CalcCritDamage(totalStat, level),
            (StatType.CriticalHit, _) => StatEquations.CalcCritRate(totalStat, level),
            (StatType.DirectHitRate, _) => StatEquations.CalcDirectHitRate(totalStat, level),
            (StatType.Determination, _) => StatEquations.CalcDeterminationMultiplier(totalStat, level),
            //Outgoing DMG
            (StatType.Tenacity, 1) => 1f + StatEquations.CalcTenacityModifier(totalStat, level),
            //Incoming DMG
            (StatType.Tenacity, _) => 1f - StatEquations.CalcTenacityModifier(totalStat, level),
            (StatType.Piety, _) => StatEquations.CalcMPPerSecond(totalStat, level),
            //AA/DoT Multiplier
            (StatType.SkillSpeed or StatType.SpellSpeed, 1) => StatEquations.CalcAADotMultiplier(totalStat, level),
            //GCD
            (StatType.SkillSpeed or StatType.SpellSpeed, _) => StatEquations.CalcGCD(totalStat, level),
            (StatType.Defense or StatType.MagicDefense, _) => StatEquations.CalcDefenseMitigation(totalStat, level),
            //ToDO: Still rounding issues
            (StatType.Vitality, _) => StatEquations.CalcHP(totalStat, level, curClass.ClassJob),
            (StatType.MagicalDamage, _) or (StatType.PhysicalDamage, _)
                => StatEquations.CalcAvarageDamage(
                    getStat(StatType.PhysicalDamage),
                    getStat((StatType)curClass.ClassJob.PrimaryStat),
                    getStat(StatType.CriticalHit),
                    getStat(StatType.DirectHitRate),
                    getStat(StatType.Determination),
                    getStat(StatType.Tenacity),
                    level,
                    curClass.ClassJob),
            _ => float.NaN
        };
    }
}
