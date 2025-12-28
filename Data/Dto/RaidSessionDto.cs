using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data.Dto;

public class RaidSessionDto(RaidSession session)
{

    public DateTime StartTime = session.StartTime;

    public TimeSpan Duration = session.Duration;

    public IList<ParticipantDto> Participants = session.Participants.Select(p => p.ToDto()).ToList();

    public Reference<Character>? Organizer = session.Organizer;

    public RaidGroup? Group = session.Group;

    public EventStatus Status;

    public IList<InstanceSessionDto> PlannedContent = session.PlannedContent.Select(c => c.ToDto()).ToList();

    public string Title = string.Empty;
}

public sealed class ParticipantDto(Participant participant)
{
    public readonly Reference<Character> Character = participant.Character;
    public InviteStatus InvitationStatus = participant.InvitationStatus;
    public ParticipationStatus ParticipationStatus = participant.ParticipationStatus;
}

public sealed class InstanceSessionDto(InstanceSession session)
{
    public uint InstanceId = session.Instance.InstanceId;
    public PlannedStatus Plan = session.Plan;
    public bool Tried = session.Tried;
    public bool Killed = session.Killed;
    public float BestPercentage = session.BestPercentage;
    public Dictionary<HrtId, List<Item>> Loot { get; } = session.Loot;
}