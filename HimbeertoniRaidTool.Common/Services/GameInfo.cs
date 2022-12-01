using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Data;

namespace HimbeertoniRaidTool.Common.Services;

public class GameInfo
{
    private readonly GameExpansion[] Expansions = new GameExpansion[6];
    private readonly Dictionary<uint, InstanceWithLoot> InstanceDB;
    public GameExpansion CurrentExpansion => Expansions[5];
    public GameInfo(CuratedData curatedData)
    {
        InstanceDB = new();
        foreach (var instance in curatedData.InstanceDB)
            InstanceDB.Add(instance.InstanceID, instance);
        Expansions[5] = curatedData.CurrentExpansion;
    }
    public InstanceWithLoot GetInstance(uint instanceID) => InstanceDB[instanceID];
    public IEnumerable<InstanceWithLoot> GetInstances() => InstanceDB.Values;
}
