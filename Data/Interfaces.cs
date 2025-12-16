using HimbeertoniRaidTool.Common.Security;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public interface IConvertibleToDto<out TDto>
{
    public TDto ToDto();
}

public interface ICreatableFromDto<out TData, in TDto>
{
    public static abstract TData FromDto(TDto dto);
}

public interface IUpdatableFromDto<in TDto>
{
    public void UpdateFromDto(TDto dto);
}

public interface IHasDtoIsCreatable<out TData, TDto> : ICreatableFromDto<TData, TDto>, IConvertibleToDto<TDto>;
public interface IHasDtoIsUpdatable<TDto> : IUpdatableFromDto<TDto>, IConvertibleToDto<TDto>;

public interface IHasDtoIsCreatableAndUpdatable<out TData, TDto> : IHasDtoIsCreatable<TData, TDto>,
                                                                   IHasDtoIsUpdatable<TDto>;

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

public interface IHrtDataTypeWithId<TData, TDto> : IHrtDataTypeWithId, IHasHrtId<TData>, IHasDtoIsUpdatable<TDto>
    where TData : IHasHrtId<TData>
{
    static abstract TData Empty { get; }
}

public interface ICloneable<out TData>
{
    TData Clone();
}