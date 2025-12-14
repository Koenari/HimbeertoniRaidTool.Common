using HimbeertoniRaidTool.Common.Security;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReadOnlyGearSet
{
    GearItem this[GearSetSlot slot] { get; }

    FoodItem? Food { get; }
    int GetStat(StatType type);
    GearSetStatBlock GetStatBlock(PlayableClass job, Tribe? tribe = null, PartyBonus bonus = PartyBonus.None);
}

public interface IHrtDataType
{
    static abstract string DataTypeName { get; }
    string Name { get; }
}

public interface IHrtDataTypeWithId : IHrtDataType, IHasHrtId;

public interface IHrtDataTypeWithId<T> : IHrtDataTypeWithId, IHasHrtId<T> where T : IHasHrtId<T>
{
    static abstract T Empty { get; }
}

public interface ICloneable<out TData>
{
    TData Clone();
}