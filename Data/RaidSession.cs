using System.Diagnostics.CodeAnalysis;
using HimbeertoniRaidTool.Common.Data.Dto;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidSession : IHrtDataTypeWithId<RaidSession, RaidSessionDto>, ICloneable<RaidSession>
{
    public static string DataTypeName => "raid session";
    public string Name => Title.Length > 0 ? Title : $"{Group?.Name} @ {StartTime:f}";

    public static HrtId.IdType IdType => HrtId.IdType.RaidSession;


    #region Serialized

    [JsonProperty("LocalId", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = [];

    [JsonProperty("StartTime")] public DateTime StartTime { get; set; }

    [JsonProperty("Duration")] public TimeSpan Duration { get; set; }

    [JsonProperty("Participants", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    private readonly List<Participant> _participants = [];

    [JsonProperty("Owner")] public Reference<Character>? Organizer;

    [JsonProperty("Group")] public RaidGroup? Group;

    [JsonProperty("Status")] public EventStatus Status;

    [JsonProperty("PlannedContent")] private readonly List<InstanceSession> _plannedContent = [];

    [JsonProperty("Title")] public string Title = string.Empty;

    #endregion

    [JsonIgnore] public DateTime EndTime => StartTime + Duration;
    [JsonIgnore] IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;
    [JsonIgnore] public IEnumerable<Participant> Participants => _participants;

    [JsonIgnore] public int NumParticipants => _participants.Count;

    [JsonIgnore] public IReadOnlyList<InstanceSession> PlannedContent => _plannedContent;

    [JsonConstructor]
    public RaidSession() : this(DateTime.Now) { }
    public RaidSession(DateTime startTime) : this(startTime, TimeSpan.FromHours(1)) { }

    public RaidSession(DateTime startTime, TimeSpan duration, Reference<Character>? organizer = null,
                       RaidGroup? group = null)
    {
        StartTime = startTime;
        Duration = duration;
        Organizer = organizer;
        if (Organizer is not null && Invite(Organizer, out var p))
        {
            p.InvitationStatus = InviteStatus.Confirmed;
        }
        Group = group;
        if (Group == null)
            return;
        foreach (var player in Group)
        {
            if (player.MainChar == Organizer?.Data) continue;
            Invite(new Reference<Character>(player.MainChar), out _);
        }
    }

    public Participant? this[Character character] =>
        Participants.FirstOrDefault(p => character.Equals(p?.Character.Data), null);

    public bool IsOrganizer(IHasHrtId player) => Organizer is not null && player.LocalId == Organizer.Data.LocalId;

    public bool Invite(Reference<Character> newParticipant, [NotNullWhen(true)] out Participant? participant)
    {
        participant = null;
        if (_participants.Any(p => p.Character.Equals(newParticipant)) || newParticipant.Data.LocalId.IsEmpty)
            return false;
        participant = new Participant(newParticipant);
        _participants.Add(participant);
        foreach (var instanceSession in PlannedContent)
        {
            instanceSession.Loot.Add(participant.Character.Id, []);
        }
        return true;
    }
    public void Uninvite(Reference<Character> toDelete) => _participants.RemoveAll(p => p.Character.Equals(toDelete));

    public void AddInstance(InstanceSession instance)
    {
        if (PlannedContent.Any(i => i.Instance == instance.Instance)) return;
        foreach (var participant in _participants)
        {
            instance.Loot.Add(participant.Character.Id, []);
        }
        _plannedContent.Add(instance);
    }

    public void RemoveInstance(InstanceSession instance) =>
        _plannedContent.RemoveAll(i => i.Instance == instance.Instance);

    public RaidSession Clone() => CloneService.Clone(this);

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);


    public void CopyFrom(RaidSession dataCopy)
    {
        Title = dataCopy.Title;
        StartTime = dataCopy.StartTime;
        Duration = dataCopy.Duration;
        _participants.Clear();
        _participants.AddRange(dataCopy._participants);
        Organizer = dataCopy.Organizer;
        Group = dataCopy.Group;
        Status = dataCopy.Status;
        _plannedContent.Clear();
        _plannedContent.AddRange(dataCopy.PlannedContent);
    }

    public override string ToString()
    {
        return Title.Length > 0 ? $"{Title} ({Group?.Name} @ {StartTime:f})"
            : $"{Group?.Name} @ {StartTime:f}";
    }


    public RaidSessionDto ToDto() => new(this);
    public void UpdateFromDto(RaidSessionDto dto) => throw new NotImplementedException();
}

[JsonObject(MemberSerialization.OptIn)]
public class InstanceSession : IConvertibleToDto<InstanceSessionDto>
{

    #region Serialized

    [JsonProperty("InstanceId")] private readonly uint _instanceId;
    [JsonProperty("Plan")] public PlannedStatus Plan;
    [JsonProperty("Tried")] public bool Tried;
    [JsonProperty("Killed")] public bool Killed;
    [JsonProperty("BestPercentage")] public float BestPercentage;

    [JsonProperty("Loot")] public Dictionary<HrtId, List<Item>> Loot { get; } = [];

    #endregion

    public InstanceSession(InstanceWithLoot instance)
    {
        _instanceId = instance.InstanceId;
    }
    [JsonConstructor]
    private InstanceSession(uint instanceId)
    {
        _instanceId = instanceId;
    }
    public InstanceWithLoot Instance => GameInfo.GetInstance(_instanceId);



    public enum PlannedStatus
    {
        Unknown = 0,
        Planned = 1,
        NotPlanned = 2,
        SafeKill = 3,
        Kill = 4,
        Progress = 5,
    }

    public InstanceSessionDto ToDto() => new(this);
    public void UpdateFromDto(InstanceSessionDto dto) => throw new NotImplementedException();
}

[JsonObject(MemberSerialization.OptIn)]
public class Participant(Reference<Character> character) : IConvertibleToDto<ParticipantDto>
{
    #region Serialized

    [JsonProperty("Player", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public readonly Reference<Character> Character = character;
    [JsonProperty("InvitationStatus")] public InviteStatus InvitationStatus = InviteStatus.NoStatus;
    [JsonProperty("ParticipationStatus")] public ParticipationStatus ParticipationStatus = ParticipationStatus.NoStatus;
    [JsonProperty("Loot")] public readonly HashSet<GearItem> ReceivedLoot = [];

    [JsonConstructor]
    private Participant() : this(new Reference<Character>(HrtId.Empty, _ => null))
    {
    }

    #endregion


    public ParticipantDto ToDto() => new(this);
    public void UpdateFromDto(ParticipantDto dto) => throw new NotImplementedException();
}