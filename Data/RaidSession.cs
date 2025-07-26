using System.Diagnostics.CodeAnalysis;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidSession : IHrtDataTypeWithId<RaidSession>, ICloneable<RaidSession>
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

    [JsonProperty("Owner")] public Reference<Player>? Organizer;

    [JsonProperty("Group")] public RaidGroup? Group;

    [JsonProperty("Status")] public EventStatus Status;

    [JsonProperty("PlannedContent")] public readonly List<InstanceSession> PlannedContent = [];

    [JsonProperty("Title")] public string Title = string.Empty;

    #endregion

    [JsonIgnore] public DateTime EndTime => StartTime + Duration;
    [JsonIgnore] IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;
    [JsonIgnore] public IEnumerable<Participant> Participants => _participants;

    [JsonIgnore] public int NumParticipants => _participants.Count;

    [JsonConstructor]
    public RaidSession() : this(DateTime.Now) { }
    public RaidSession(DateTime startTime) : this(startTime, TimeSpan.FromHours(1)) { }

    public RaidSession(DateTime startTime, TimeSpan duration, Reference<Player>? organizer = null,
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
            if (player == Organizer?.Data) continue;
            Invite(new Reference<Player>(player), out _);
        }
    }

    public Participant? this[Player player] => Participants.FirstOrDefault(p => player == p?.Player.Data, null);

    public bool IsOrganizer(IHasHrtId player) => Organizer is not null && player.LocalId == Organizer.Data.LocalId;

    public bool Invite(Reference<Player> newParticipant, [NotNullWhen(true)] out Participant? participant)
    {
        participant = null;
        if (_participants.Any(p => p.Player.Equals(newParticipant)) || newParticipant.Data.LocalId.IsEmpty)
            return false;
        participant = new Participant(newParticipant);
        _participants.Add(participant);
        return true;
    }

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
        PlannedContent.Clear();
        PlannedContent.AddRange(dataCopy.PlannedContent);
    }

    public override string ToString()
    {
        return Title.Length > 0 ? $"{Title} ({Group?.Name} @ {StartTime:f})"
            : $"{Group?.Name} @ {StartTime:f}";
    }

}

[JsonObject(MemberSerialization.OptIn)]
public class InstanceSession
{


    #region Serialized

    [JsonProperty("InstanceId")] private readonly uint _instanceId;
    [JsonProperty("Plan")] public PlannedStatus Plan;
    [JsonProperty("Tried")] public bool Tried;
    [JsonProperty("Killed")] public bool Killed;
    [JsonProperty("BestPercentage")] public float BestPercentage;

    [JsonProperty("Loot")] public Dictionary<Participant, List<Item>> Loot { get; } = [];

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
}

[JsonObject(MemberSerialization.OptIn)]
public class Participant(Reference<Player> player)
{
    #region Serialized

    [JsonProperty("Player", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public readonly Reference<Player> Player = player;
    [JsonProperty("InvitationStatus")] public InviteStatus InvitationStatus = InviteStatus.NoStatus;
    [JsonProperty("ParticipationStatus")] public ParticipationStatus ParticipationStatus = ParticipationStatus.NoStatus;
    [JsonProperty("Loot")] public readonly HashSet<GearItem> ReceivedLoot = [];

    [JsonConstructor]
    private Participant() : this(new Reference<Player>(HrtId.Empty, _ => null))
    {
    }

    #endregion


}