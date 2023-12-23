using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Data;

namespace HimbeertoniRaidTool.Common.Services;

public class GameInfo
{
    private readonly GameExpansion[] _expansions;
    private readonly Dictionary<uint, InstanceWithLoot> _instanceDb;
    public GameExpansion CurrentExpansion => _expansions[^1];
    internal GameInfo(CuratedData curatedData)
    {
        _instanceDb = new Dictionary<uint, InstanceWithLoot>();
        foreach (InstanceWithLoot? instance in curatedData.InstanceDb)
        {
            _instanceDb.Add(instance.InstanceId, instance);
        }
        _expansions = curatedData.Expansions;
    }
    public GameExpansion GetExpansionByLevel(int level) => _expansions.First(exp => exp.MaxLevel >= level);
    public InstanceWithLoot GetInstance(uint instanceId) => _instanceDb[instanceId];
    public IEnumerable<InstanceWithLoot> GetInstances() => _instanceDb.Values;
}