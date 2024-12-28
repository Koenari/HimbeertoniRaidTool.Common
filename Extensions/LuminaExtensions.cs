using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Extensions;

public static class TribeExtensions
{
    public static int GetRacialModifier(this Tribe t, StatType type) => type switch
    {
        StatType.Strength     => t.STR,
        StatType.Dexterity    => t.DEX,
        StatType.Vitality     => t.VIT,
        StatType.Intelligence => t.INT,
        StatType.Mind         => t.MND,
        _                     => 0,
    };
}

public static class EquipSlotCategoryExtensions
{
    public static bool Contains(this EquipSlotCategory self, GearSetSlot slot) => self.HasValueAt(slot, 1);

    public static bool Disallows(this EquipSlotCategory self, GearSetSlot slot) => self.HasValueAt(slot, -1);

    public static bool HasValueAt(this EquipSlotCategory self, GearSetSlot slot, sbyte value)
    {
        return slot switch
        {
            GearSetSlot.MainHand    => self.MainHand == value,
            GearSetSlot.Head        => self.Head == value,
            GearSetSlot.Body        => self.Body == value,
            GearSetSlot.Hands       => self.Gloves == value,
            GearSetSlot.Waist       => self.Waist == value,
            GearSetSlot.Legs        => self.Legs == value,
            GearSetSlot.Feet        => self.Feet == value,
            GearSetSlot.OffHand     => self.OffHand == value,
            GearSetSlot.Ear         => self.Ears == value,
            GearSetSlot.Neck        => self.Neck == value,
            GearSetSlot.Wrist       => self.Wrists == value,
            GearSetSlot.Ring1       => self.FingerR == value,
            GearSetSlot.Ring2       => self.FingerL == value,
            GearSetSlot.SoulCrystal => self.SoulCrystal == value,
            _                       => false,
        };
    }

    public static IEnumerable<GearSetSlot> AvailableSlots(this EquipSlotCategory self)
    {
        for (int i = 0; i < (int)GearSetSlot.SoulCrystal; i++)
        {
            if (self.Contains((GearSetSlot)i))
                yield return (GearSetSlot)i;
        }
    }
}

public static class ClassJobCategoryExtensions
{
    public static IEnumerable<Job> ToJob(this ClassJobCategory self) =>
        Enum.GetValues<Job>().Where(job => self.Contains(job));

    public static bool Contains(this ClassJobCategory cat, Job job) => job switch
    {
        Job.ADV => cat.ADV,
        Job.AST => cat.AST,
        Job.BLM => cat.BLM,
        Job.BLU => cat.BLU,
        Job.BRD => cat.BRD,
        Job.DNC => cat.DNC,
        Job.DRG => cat.DRG,
        Job.DRK => cat.DRK,
        Job.GNB => cat.GNB,
        Job.MCH => cat.MCH,
        Job.MNK => cat.MNK,
        Job.NIN => cat.NIN,
        Job.PLD => cat.PLD,
        Job.RDM => cat.RDM,
        Job.RPR => cat.RPR,
        Job.SAM => cat.SAM,
        Job.SCH => cat.SCH,
        Job.SGE => cat.SGE,
        Job.SMN => cat.SMN,
        Job.WAR => cat.WAR,
        Job.WHM => cat.WHM,
        Job.GLA => cat.GLA,
        Job.MRD => cat.MRD,
        Job.LNC => cat.LNC,
        Job.PGL => cat.PGL,
        Job.ARC => cat.ARC,
        Job.THM => cat.THM,
        Job.ACN => cat.ACN,
        Job.CNJ => cat.CNJ,
        Job.ROG => cat.ROG,
        Job.VPR => cat.VPR,
        Job.PCT => cat.PCT,
        _       => false,
    };
}