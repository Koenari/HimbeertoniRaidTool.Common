using System;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Security;
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HrtID : IEquatable<HrtID>, IComparable<HrtID>
{
    public readonly static HrtID Empty = new(0, IDType.None, 0);
    public static HrtID FromString(string id)
    {
        string[] parts = id.Split('-');
        if (parts.Length != 5 || parts[0] != "HRT" || parts[1] != "1")
            return Empty;
        try
        {
            uint authority = uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
            IDType type = Enum.Parse<IDType>(parts[3]);
            ulong sequence = ulong.Parse(parts[4], System.Globalization.NumberStyles.HexNumber);
            return new(authority, type, sequence);
        }
        catch { }
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
    public readonly IDType Type;
    [JsonProperty]
    public readonly ulong Sequence;
    [JsonProperty]
    public byte[] Signature = Array.Empty<byte>();
    [JsonIgnore] public bool IsSigned => Signature.Length > 0;
    [JsonIgnore] public bool IsEmpty => Sequence == 0;

    public HrtID(uint authority, IDType type, ulong sequence)
    {
        Authority = authority;
        Type = type;
        Sequence = sequence;
    }
    public override string ToString() => $"HRT-{Revision}-{Authority:X}-" +
        $"{Type}-{Sequence:X}";
    public bool Equals(HrtID? other) =>
        other is not null && Type == other.Type && Authority == other.Authority && Sequence == other.Sequence && Revision == other.Revision;

    public int CompareTo(HrtID? other)
    {
        if (other is null) return 1;
        if (Authority != other.Authority)
            return (Authority > other.Authority) ? 1 : -1;
        if (Sequence != other.Sequence)
            return (Sequence > other.Sequence) ? 1 : -1;
        return 0;
    }

    public enum IDType
    {
        None = 0,
        Player = 1,
        Character = 2,
        Gear = 3,
    }
    public enum AuthorityLevel
    {
        None = 0,
        Global = 1,
        Official = 2,
        Remote = 5,
        Local = 10,
    }

    public override bool Equals(object? obj) => Equals(obj as HrtID);
    public static bool operator ==(HrtID left, HrtID right) => left.Equals(right);
    public static bool operator !=(HrtID left, HrtID right) => !(left == right);
    public static bool operator <(HrtID left, HrtID right) => left.CompareTo(right) < 0;
    public static bool operator <=(HrtID left, HrtID right) => left.CompareTo(right) <= 0;
    public static bool operator >(HrtID left, HrtID right) => left.CompareTo(right) > 0;
    public static bool operator >=(HrtID left, HrtID right) => left.CompareTo(right) >= 0;
}
