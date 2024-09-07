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
    public static double EvaluateStat(StatType type, IStatEquations stats,
                                      int alternative = 0) => (type, alternative) switch
    {
        (StatType.CriticalHit, 1)   => stats.CritDamage(),
        (StatType.CriticalHit, _)   => stats.CritChance(),
        (StatType.DirectHitRate, _) => stats.DirectHitChance(),
        (StatType.Determination, _) => stats.DeterminationMultiplier(),
        //Outgoing DMG
        (StatType.Tenacity, 1) => stats.TenacityOffensiveModifier(),
        //Incoming DMG
        (StatType.Tenacity, _) => stats.TenacityDefensiveModifier(),
        (StatType.Piety, _)    => stats.MpPerTick(),
        //AA/DoT Multiplier
        (StatType.SkillSpeed or StatType.SpellSpeed, 1) => stats.DotMultiplier(),
        //GCD
        (StatType.SkillSpeed or StatType.SpellSpeed, _) => stats.Gcd(),
        (StatType.Defense, _)                           => stats.PhysicalDefenseMitigation(),
        (StatType.MagicDefense, _)                      => stats.MagicalDefenseMitigation(),
        (StatType.Vitality, _)                          => stats.MaxHp(),
        (StatType.PhysicalDamage, _)                    => stats.AverageSkillDamagePerSecond(100),
        (StatType.MagicalDamage, _)                     => stats.AverageSkillDamagePerSecond(100),
        _                                               => float.NaN,
    };
}