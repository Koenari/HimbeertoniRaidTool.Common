using System.Runtime.CompilerServices;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReference
{
    public HrtId Id { get; }
}

public class Reference<T>(HrtId id,Func<HrtId,T?> getObject) : IReference, IEquatable<Reference<T>> where T : class, IHasHrtId<T>, new()
{
    private static T Default = new T();
    private T? _cache;
    private Func<HrtId, T?> getter { get; } = getObject;
    
    public HrtId Id { get; }= id;
    public bool Loaded => _cache is not null;
    public T Data
    {
        get
        {
            _cache ??= getter(Id);
            return _cache  ?? Default;
        }
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as Reference<T>);
    public bool Equals(Reference<T>? other) => this.Id.Equals(other?.Id);
    public Reference(T obj) : this(obj.LocalId, _ => obj){}
    
    public static implicit operator T(Reference<T> obj) => obj.Data;
    public static implicit operator Reference<T>(T obj) => new Reference<T>(obj);
}