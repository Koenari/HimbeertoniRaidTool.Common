namespace HimbeertoniRaidTool.Common.Data.Dto;

public class UserDto(User user)
{

    public readonly string Username = user.Username;

    public readonly string DisplayName = user.DisplayName;
}