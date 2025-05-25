using System.Diagnostics.CodeAnalysis;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
public class RaidSession : IHrtDataTypeWithId
{
    public static string DataTypeNameStatic = "session";
    public string DataTypeName => DataTypeNameStatic;
    public string Name => Title.Length > 0 ? Title : $"{Group?.Name} @ {StartTime:f}";

    public HrtId.IdType IdType => HrtId.IdType.RaidSession;

    [JsonProperty("LocalId", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = [];

    [JsonIgnore] IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    [JsonProperty("StartTime")] public DateTime StartTime { get; set; }
    [JsonProperty("Duration")] public TimeSpan Duration { get; set; }

    [JsonIgnore] public DateTime EndTime => StartTime + Duration;

    [JsonProperty("Participants")] private readonly HashSet<Participant> _participants = [];

    [JsonProperty("Owner")] public Player? Organizer;
    [JsonProperty("Group")] public RaidGroup? Group;
    [JsonProperty("Status")] public EventStatus Status;
    [JsonProperty("PlannedContent")] public List<InstanceSession> PlannedContent = [];
    [JsonProperty("Title")] public string Title = string.Empty;
    [JsonIgnore] public IEnumerable<Participant> Participants => _participants;


    [JsonConstructor]
    private RaidSession(Player organizer)
    {
        Organizer = organizer;
    }

    public RaidSession() : this(DateTime.Now) { }
    public RaidSession(DateTime startTime) : this(startTime, TimeSpan.FromHours(1)) { }

    public RaidSession(DateTime startTime, TimeSpan duration, Player? organizer = null, RaidGroup? group = null)
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
            if (player == Organizer) continue;
            Invite(player, out _);
        }
    }

    public Participant? this[Player player] => Participants.FirstOrDefault(p => player.Equals(p?.Player), null);

    public bool IsOrganizer(IHasHrtId player) => Organizer is not null && player.LocalId == Organizer.LocalId;

    public bool Invite(Player newParticipant, [NotNullWhen(true)] out Participant? participant)
    {
        participant = null;
        if (_participants.Any(p => p.Player.Equals(newParticipant)) || newParticipant.LocalId.IsEmpty) return false;
        participant = new Participant(newParticipant);
        return _participants.Add(participant);
    }

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);


    public void CopyFrom(RaidSession dataCopy) => Title = dataCopy.Title;
}

[JsonObject(MemberSerialization.OptIn)]
[method: JsonConstructor]
public class InstanceSession(InstanceWithLoot instance)
{
    #region Serialized

    [JsonProperty("InstanceId")] private readonly uint _instanceId = instance.InstanceId;
    [JsonProperty("Plan")] public PlannedStatus Plan;
    [JsonProperty("Tried")] public bool Tried;
    [JsonProperty("Killed")] public bool Killed;
    [JsonProperty("BestPercentage")] public float BestPercentage;
    [JsonProperty("Loot")] public Dictionary<Participant, List<Item>> Loot { get; } = [];

    #endregion

    public InstanceWithLoot Instance => GameInfo.GetInstance(_instanceId);



    public enum PlannedStatus
    {
        Unknown = 0,
        Planned = 1,
        NotPlanned = 2,
        SafeKill = 2,
        Kill = 3,
        Progress = 4,
    }
}

[JsonObject(MemberSerialization.OptIn)]
[method: JsonConstructor]
public class Participant(Player player)
{
    #region Serialized

    [JsonProperty("Player")] public readonly Player Player = player;
    [JsonProperty("InviteStatus")] public InviteStatus InvitationStatus = InviteStatus.NoStatus;
    [JsonProperty("ParticipationStatus")] public ParticipationStatus ParticipationStatus = ParticipationStatus.NoStatus;
    [JsonProperty("Loot")] public readonly HashSet<GearItem> ReceivedLoot = [];

    #endregion


}