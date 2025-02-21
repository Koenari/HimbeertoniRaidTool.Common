using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public interface IHrtDataType : ICloneable
{
    public string DataTypeName { get; }
    public string Name { get; }
}

public interface IHrtDataTypeWithId : IHrtDataType, IHasHrtId;
public interface ICloneable;