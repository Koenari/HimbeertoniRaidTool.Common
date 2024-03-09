using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HimbeertoniRaidTool.Common.Data;
using HimbeertoniRaidTool.Common.Security;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

public class RaidSession : IHrtDataTypeWithId
{
    public static string DataTypeNameStatic = "session";
    public string DataTypeName => DataTypeNameStatic;
    public string Name => $"{Group?.Name} @ {StartTime:f}";

    public HrtId.IdType IdType => HrtId.IdType.RaidSession;

    [JsonProperty("LocalId", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();

    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    [JsonProperty("StartTime")] public DateTime StartTime { get; private set; }
    [JsonProperty("Duration")] public TimeSpan Duration { get; private set; }

    [JsonIgnore] public DateTime EndTime => StartTime + Duration;

    [JsonProperty("Participants")] private readonly HashSet<Participant> _participants = new();

    [JsonProperty("Owner")] public readonly Player Organizer;
    [JsonProperty("Group")] public RaidGroup? Group;
    [JsonProperty("Status")] public EventStatus Status;
    [JsonProperty("PlannedContent")] public List<InstanceWithLoot> PlannedContent = new();
    [JsonProperty("Title")] public string Title = string.Empty;
    [JsonIgnore] public IEnumerable<Participant> Participants => _participants;

    [JsonConstructor]
    private RaidSession(Player organizer)
    {
        Organizer = organizer;
    }

    public RaidSession(Player organizer, DateTime startTime) : this(organizer, startTime, TimeSpan.FromHours(1)) { }

    public RaidSession(Player organizer, DateTime startTime, TimeSpan duration, RaidGroup? group = null)
    {
        StartTime = startTime;
        Duration = duration;
        Organizer = organizer;
        if (Invite(organizer, out Participant? p))
        {
            p.InvitationStatus = InviteStatus.Confirmed;
        }
        Group = group;
        if (group != null)
            foreach (Player player in group)
            {
                if (player == organizer) continue;
                Invite(player, out _);
            }
    }

    public Participant? this[Player player] => Participants.FirstOrDefault(p => player.Equals(p?.Player), null);

    public bool IsOrganizer(IHasHrtId player) => player.LocalId == Organizer.LocalId;

    public bool Invite(Player newParticipant, [NotNullWhen(true)] out Participant? participant)
    {
        participant = null;
        if (_participants.Any(p => p.Player.Equals(newParticipant)) || newParticipant.LocalId.IsEmpty) return false;
        participant = new Participant(newParticipant);
        return _participants.Add(participant);
    }

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);


}

[JsonObject(MemberSerialization.OptIn)]
public class Participant
{
    [JsonProperty("Player")] public readonly Player Player;
    [JsonProperty("InviteStatus")] public InviteStatus InvitationStatus = InviteStatus.NoStatus;
    [JsonProperty("WasPresent")] public bool WasPresent = false;
    [JsonProperty("WasExcused")] public bool WasExcused = false;
    [JsonProperty("Loot")] public readonly HashSet<GearItem> ReceivedLoot = new();

    [JsonConstructor]
    public Participant(Player player)
    {
        Player = player;
    }
}