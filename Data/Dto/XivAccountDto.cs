namespace HimbeertoniRaidTool.Common.Data.Dto;

public class XivAccountDto(XivAccount account)
{
    public readonly string Name = account.Name;

    public readonly ulong HashedId = account.HashedId;
}