using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public interface IHrtDataType
{
    public string DataTypeName { get; }
    public string Name { get; }
}

public interface IHrtDataTypeWithId : IHrtDataType, IHasHrtId
{
}