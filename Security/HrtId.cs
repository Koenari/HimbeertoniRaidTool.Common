using System.Globalization;

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
        if (!uint.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint authority))
            return Empty;
        if (!Enum.TryParse<IdType>(parts[3], true, out var type))
            return Empty;
        if (!ulong.TryParse(parts[4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong sequence))
            return Empty;
        return new HrtId(authority, type, sequence);

    }
    [JsonProperty]
    public readonly byte Revision = 1;
    [JsonProperty]
    public readonly uint Authority;
    [JsonIgnore]
    public AuthorityLevel Level => Authority switch
    {
        > 0x7FFFFFFF => AuthorityLevel.Local,
        > 0xFFFF     => AuthorityLevel.Remote,
        > 0x1        => AuthorityLevel.Official,
        1            => AuthorityLevel.Global,
        0            => AuthorityLevel.None,
    };
    [JsonProperty]
    public readonly IdType Type;
    [JsonProperty]
    public readonly ulong Sequence;
    [JsonProperty]
    public byte[] Signature = [];
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
        other is not null && Type == other.Type && Authority == other.Authority && Sequence == other.Sequence
     && Revision == other.Revision;

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
        RaidSession = 5,
        User = 6,
        XivAccount = 7,
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
    public static bool operator ==(HrtId left, HrtId right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(HrtId left, HrtId right)
    {
        return !(left == right);
    }
    public static bool operator <(HrtId left, HrtId right)
    {
        return left.CompareTo(right) < 0;
    }
    public static bool operator <=(HrtId left, HrtId right)
    {
        return left.CompareTo(right) <= 0;
    }
    public static bool operator >(HrtId left, HrtId right)
    {
        return left.CompareTo(right) > 0;
    }
    public static bool operator >=(HrtId left, HrtId right)
    {
        return left.CompareTo(right) >= 0;
    }
    public static explicit operator HrtId(string id)
    {
        return FromString(id);
    }
}

public interface IHasHrtId
{
    public HrtId.IdType IdType { get; }
    public HrtId LocalId { get; set; }

    /// <summary>
    ///     XIV Raid Tool specific unique IDs used for remote storage and lookup.
    /// </summary>
    public IList<HrtId> RemoteIds { get; }
}

public interface IHasHrtId<T> : IHasHrtId where T : IHasHrtId<T>
{
    public static virtual bool operator ==(T obj1, T obj2)
    {
        return obj1.LocalId == obj2.LocalId;
    }
    public static virtual bool operator !=(T obj1, T obj2)
    {
        return !(obj1 == obj2);
    }
}