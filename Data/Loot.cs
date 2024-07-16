using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HimbeertoniRaidTool.Common.Data;

public class InstanceWithLoot
{
    private static readonly ExcelSheet<InstanceContent>? _instanceSheet =
        ServiceManager.ExcelModule.GetSheet<InstanceContent>();
    private static readonly ExcelSheet<ContentFinderCondition>? _contentFinderSheet =
        ServiceManager.ExcelModule.GetSheet<ContentFinderCondition>();
    private static readonly Dictionary<uint, uint> _contentFinderLookup;
    static InstanceWithLoot()
    {
        _contentFinderLookup = new Dictionary<uint, uint>();
        if (_contentFinderSheet == null) return;
        foreach (ContentFinderCondition? row in _contentFinderSheet.Where(x => x.ContentLinkType == 1))
        {
            _contentFinderLookup.TryAdd(row.Content, row.RowId);
        }
    }
    public bool IsAvailable => _contentFinderCondition is not null;
    public InstanceType InstanceType => (InstanceType)_instanceContent.InstanceContentType;

    public EncounterDifficulty Difficulty { get; }
    public string Name => _contentFinderCondition?.Name ?? CommonLoc.NotAvail_Abbrev;
    public IEnumerable<HrtItem> PossibleItems { get; }
    public IEnumerable<HrtItem> GuaranteedItems { get; }
    public uint InstanceId => _instanceContent.RowId;
    private readonly InstanceContent _instanceContent;
    private readonly ContentFinderCondition? _contentFinderCondition;
    public IEnumerable<HrtItem> AllLoot
    {
        get
        {
            foreach (HrtItem? item in PossibleItems)
            {
                yield return item;
            }
            foreach (HrtItem? item in GuaranteedItems)
            {
                yield return item;
            }
        }
    }
    public InstanceWithLoot(uint id, EncounterDifficulty difficulty = EncounterDifficulty.Normal,
                            ItemIdCollection? possibleLoot = null, ItemIdCollection? guaranteedLoot = null)
    {
        _instanceContent = _instanceSheet?.GetRow(id) ?? new InstanceContent();
        _contentFinderCondition = _contentFinderLookup.TryGetValue(id, out uint contentId)
            ? _contentFinderSheet?.GetRow(contentId)! : null;
        Difficulty = difficulty;
        GuaranteedItems = (guaranteedLoot ?? ItemIdCollection.Empty).Select((selectId, _) => new HrtItem(selectId));
        PossibleItems = (possibleLoot ?? ItemIdCollection.Empty).Select((selectId, _) => new HrtItem(selectId));
    }
}