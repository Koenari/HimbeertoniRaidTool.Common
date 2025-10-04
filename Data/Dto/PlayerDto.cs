using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data.Dto;

public class PlayerDto(Player player)
{
    public string NickName = player.NickName;

}