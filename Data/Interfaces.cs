using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public interface IHrtDataType : IHasHrtId
{
    public string DataTypeName { get; }
}