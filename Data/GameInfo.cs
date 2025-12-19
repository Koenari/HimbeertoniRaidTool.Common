namespace HimbeertoniRaidTool.Common.Data;

public static class GameInfo
{
    private static readonly Dictionary<uint, InstanceWithLoot> _instanceDb;
    public static GameExpansion CurrentExpansion => Expansions[^1];

    public static RaidTier? CurrentSavageTier => CurrentExpansion.CurrentSavage;
    public static RaidTier? PreviousSavageTier => CurrentExpansion.PreviousSavage ?? Expansions[^2].CurrentSavage;

    public static IReadOnlyList<GameExpansion> Expansions { get; }
    static GameInfo()
    {
        var curatedData = new CuratedData();
        _instanceDb = new Dictionary<uint, InstanceWithLoot>();
        foreach (var instance in curatedData.InstanceData)
        {
            _instanceDb.Add(instance.InstanceId, instance);
        }
        Expansions = curatedData.Expansions;
    }
    public static GameExpansion GetExpansionByLevel(int level) => Expansions.First(exp => exp.MaxLevel >= level);
    public static InstanceWithLoot GetInstance(uint instanceId) => _instanceDb[instanceId];
    public static IEnumerable<InstanceWithLoot> GetInstances() => _instanceDb.Values;
}