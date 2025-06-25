using System.Runtime.CompilerServices;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

public class Reference<T>(HrtId id,Func<HrtId,T?> getObject) where T : IHasHrtId<T>, new()
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
    
    public Reference(T obj) : this(obj.LocalId, _ => obj){}
}