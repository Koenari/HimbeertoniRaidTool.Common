namespace HimbeertoniRaidTool.Common.Data.Dto;

public class RaidGroupDto(RaidGroup group)
{
    public IList<Player> Players = group.ToList();

    public string Name = group.Name;

    public Dictionary<Role, int>? RolePriority = group.RolePriority;

    public DateTime TimeStamp = group.TimeStamp;

    public GroupType Type = group.Type;
}