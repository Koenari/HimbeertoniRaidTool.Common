using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

public class InstanceWithLoot
{
    private static readonly ExcelSheet<InstanceContent> InstanceSheet =
        CommonLibrary.ExcelModule.GetSheet<InstanceContent>();
    private static readonly ExcelSheet<ContentFinderCondition> ContentFinderSheet =
        CommonLibrary.ExcelModule.GetSheet<ContentFinderCondition>();
    private static readonly Dictionary<uint, uint> ContentFinderLookup;
    static InstanceWithLoot()
    {
        ContentFinderLookup = new Dictionary<uint, uint>();
        foreach (ContentFinderCondition? row in ContentFinderSheet.Where(x => x.ContentLinkType == 1))
        {
            ContentFinderLookup.TryAdd(row.Value.Content.RowId, row.Value.RowId);
        }
    }
    public bool IsAvailable => _contentFinderCondition is not null;
    public InstanceType InstanceType => (InstanceType)_instanceContent.InstanceContentType.RowId;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public EncounterDifficulty Difficulty { get; }
    public string Name => _contentFinderCondition?.Name.ToString() ?? CommonLoc.NotAvail_Abbrev;
    public IEnumerable<Item> PossibleItems { get; }
    public IEnumerable<Item> GuaranteedItems { get; }
    public uint InstanceId => _instanceContent.RowId;
    private readonly InstanceContent _instanceContent;
    private readonly ContentFinderCondition? _contentFinderCondition;
    public IEnumerable<Item> AllLoot
    {
        get
        {
            foreach (var item in PossibleItems)
            {
                yield return item;
            }
            foreach (var item in GuaranteedItems)
            {
                yield return item;
            }
        }
    }
    public InstanceWithLoot(uint id, EncounterDifficulty difficulty, ItemIdCollection? possibleLoot = null,
                            ItemIdCollection? guaranteedLoot = null)
    {
        _instanceContent = InstanceSheet.GetRow(id);
        _contentFinderCondition = ContentFinderLookup.TryGetValue(id, out uint contentId)
            ? ContentFinderSheet.GetRow(contentId) : null;
        Difficulty = difficulty;
        GuaranteedItems = (guaranteedLoot ?? ItemIdCollection.Empty).Select((selectId, _) => new Item(selectId));
        PossibleItems = (possibleLoot ?? ItemIdCollection.Empty).Select((selectId, _) => new Item(selectId));
    }

    public override string ToString() => $"{Name} ({Difficulty})";

    public override bool Equals(object? obj) => obj is InstanceWithLoot other && InstanceId == other.InstanceId;
    public override int GetHashCode() => (int)InstanceId;

    public static bool operator ==(InstanceWithLoot? left, InstanceWithLoot? right)
    {
        return left?.InstanceId == right?.InstanceId;
    }
    public static bool operator !=(InstanceWithLoot? left, InstanceWithLoot? right)
    {
        return !(left == right);
    }
}