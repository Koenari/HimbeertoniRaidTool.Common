using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Data;

namespace HimbeertoniRaidTool.Common.Services;

public class GameInfo
{
    private readonly GameExpansion[] _expansions = new GameExpansion[6];
    private readonly Dictionary<uint, InstanceWithLoot> _instanceDb;
    public GameExpansion CurrentExpansion => _expansions[5];
    internal GameInfo(CuratedData curatedData)
    {
        _instanceDb = new Dictionary<uint, InstanceWithLoot>();
        foreach (InstanceWithLoot? instance in curatedData.InstanceDb)
            _instanceDb.Add(instance.InstanceId, instance);
        _expansions[5] = curatedData.CurrentExpansion;
    }
    public InstanceWithLoot GetInstance(uint instanceId) => _instanceDb[instanceId];
    public IEnumerable<InstanceWithLoot> GetInstances() => _instanceDb.Values;
}