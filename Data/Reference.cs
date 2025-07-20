using System.Runtime.CompilerServices;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public interface IReference
{
    public HrtId Id { get; }
}

public class Reference<T>(HrtId id,Func<HrtId,T?> getObject) : IReference where T : IHasHrtId<T>, new()
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
    [JsonConstructor]
    public Reference(T obj) : this(obj.LocalId, _ => obj){}
}