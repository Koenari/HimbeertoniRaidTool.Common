using HimbeertoniRaidTool.Common.Security;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReadOnlyGearSet
{
    GearItem this[GearSetSlot slot] { get; }

    FoodItem? Food { get; }
    int GetStat(StatType type);
    public GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None);
}

public interface IHrtDataType : ICloneable
{
    public string DataTypeName { get; }
    public string Name { get; }
}

public interface IHrtDataTypeWithId : IHrtDataType, IHasHrtId;
public interface ICloneable;