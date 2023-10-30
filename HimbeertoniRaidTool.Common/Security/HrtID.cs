using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Security;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtId : IEquatable<HrtId>, IComparable<HrtId>
{
    public static readonly HrtId Empty = new(0, IdType.None, 0);
    public static HrtId FromString(string id)
    {
        string[] parts = id.Split('-');
        if (parts is not ["HRT", "1", _, _, _])
            return Empty;
        try
        {
            uint authority = uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
            var type = Enum.Parse<IdType>(parts[3]);
            ulong sequence = ulong.Parse(parts[4], System.Globalization.NumberStyles.HexNumber);
            return new HrtId(authority, type, sequence);
        }
        catch (Exception e) when (e is ArgumentException or OverflowException or ArgumentNullException
                                      or FormatException) { }
        return Empty;
    }
    [JsonProperty]
    public readonly byte Revision = 1;
    [JsonProperty]
    public readonly uint Authority;
    [JsonIgnore]
    public AuthorityLevel Level => Authority switch
    {
        > 0x7FFFFFFF => AuthorityLevel.Local,
        > 0xFFFF => AuthorityLevel.Remote,
        > 0x1 => AuthorityLevel.Official,
        1 => AuthorityLevel.Global,
        0 => AuthorityLevel.None,
    };
    [JsonProperty]
    public readonly IdType Type;
    [JsonProperty]
    public readonly ulong Sequence;
    [JsonProperty]
    public byte[] Signature = Array.Empty<byte>();
    [JsonIgnore] public bool IsSigned => Signature.Length > 0;
    [JsonIgnore] public bool IsEmpty => Sequence == 0;

    public HrtId(uint authority, IdType type, ulong sequence)
    {
        Authority = authority;
        Type = type;
        Sequence = sequence;
    }
    public override string ToString() => $"HRT-{Revision:D}-{Authority:X}-" +
                                         $"{Type:D}-{Sequence:X}";
    public bool Equals(HrtId? other) =>
        other is not null && Type == other.Type && Authority == other.Authority && Sequence == other.Sequence && Revision == other.Revision;

    public int CompareTo(HrtId? other)
    {
        if (other is null) return 1;
        if (Authority != other.Authority)
            return Authority > other.Authority ? 1 : -1;
        if (Sequence != other.Sequence)
            return Sequence > other.Sequence ? 1 : -1;
        return 0;
    }

    public enum IdType : byte
    {
        None = 0,
        Player = 1,
        Character = 2,
        Gear = 3,
        Group = 4,
    }

    public enum AuthorityLevel
    {
        None = 0,
        Global = 1,
        Official = 2,
        Remote = 5,
        Local = 10,
    }

    public override int GetHashCode() => (int)(Sequence & int.MaxValue);
    public override bool Equals(object? obj) => Equals(obj as HrtId);
    public static bool operator ==(HrtId left, HrtId right) => left.Equals(right);
    public static bool operator !=(HrtId left, HrtId right) => !(left == right);
    public static bool operator <(HrtId left, HrtId right) => left.CompareTo(right) < 0;
    public static bool operator <=(HrtId left, HrtId right) => left.CompareTo(right) <= 0;
    public static bool operator >(HrtId left, HrtId right) => left.CompareTo(right) > 0;
    public static bool operator >=(HrtId left, HrtId right) => left.CompareTo(right) >= 0;
    public static explicit operator HrtId(string id) => FromString(id);
}

public interface IHasHrtId
{
    public HrtId.IdType IdType { get; }
    public HrtId LocalId { get; }
    public IEnumerable<HrtId> RemoteIds { get; }
}