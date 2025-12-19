using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Extensions;

public static class LuminaExtensions
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

    extension(EquipSlotCategory self)
    {
        public bool Contains(GearSetSlot slot) => self.HasValueAt(slot, 1);
        public bool Disallows(GearSetSlot slot) => self.HasValueAt(slot, -1);
        public bool HasValueAt(GearSetSlot slot, sbyte value)
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
        public IEnumerable<GearSetSlot> AvailableSlots()
        {
            for (int i = 0; i < (int)GearSetSlot.SoulCrystal; i++)
            {
                if (self.Contains((GearSetSlot)i))
                    yield return (GearSetSlot)i;
            }
        }
    }

    extension(ClassJobCategory self)
    {
        public IEnumerable<Job> ToJob() =>
            Enum.GetValues<Job>().Where(job => self.Contains(job));
        public bool Contains(Job job) => job switch
        {
            Job.ADV => self.ADV,
            Job.AST => self.AST,
            Job.BLM => self.BLM,
            Job.BLU => self.BLU,
            Job.BRD => self.BRD,
            Job.DNC => self.DNC,
            Job.DRG => self.DRG,
            Job.DRK => self.DRK,
            Job.GNB => self.GNB,
            Job.MCH => self.MCH,
            Job.MNK => self.MNK,
            Job.NIN => self.NIN,
            Job.PLD => self.PLD,
            Job.RDM => self.RDM,
            Job.RPR => self.RPR,
            Job.SAM => self.SAM,
            Job.SCH => self.SCH,
            Job.SGE => self.SGE,
            Job.SMN => self.SMN,
            Job.WAR => self.WAR,
            Job.WHM => self.WHM,
            Job.GLA => self.GLA,
            Job.MRD => self.MRD,
            Job.LNC => self.LNC,
            Job.PGL => self.PGL,
            Job.ARC => self.ARC,
            Job.THM => self.THM,
            Job.ACN => self.ACN,
            Job.CNJ => self.CNJ,
            Job.ROG => self.ROG,
            Job.VPR => self.VPR,
            Job.PCT => self.PCT,
            _       => false,
        };
    }

}