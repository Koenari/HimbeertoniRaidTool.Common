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
    private static readonly Dictionary<uint, uint> _contenFinderLookup;
    static InstanceWithLoot()
    {
        _contenFinderLookup = new Dictionary<uint, uint>();
        if (_contentFinderSheet != null)
            foreach (ContentFinderCondition? row in _contentFinderSheet.Where(x => x.ContentLinkType == 1))
            {
                _contenFinderLookup.TryAdd(row.Content, row.RowId);
            }
    }
    public InstanceType InstanceType => (InstanceType)_instanceContent.InstanceContentType;

    public EncounterDifficulty Difficulty { get; }
    public string Name => _contentFinderCondition.Name;
    public IEnumerable<HrtItem> PossibleItems { get; }
    public IEnumerable<HrtItem> GuaranteedItems { get; }
    public uint InstanceId => _instanceContent.RowId;
    private readonly InstanceContent _instanceContent;
    private readonly ContentFinderCondition _contentFinderCondition;
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
        _contentFinderCondition = _contenFinderLookup.TryGetValue(id, out uint contentId)
            ? _contentFinderSheet!.GetRow(contentId)! : new ContentFinderCondition();
        Difficulty = difficulty;
        GuaranteedItems = (guaranteedLoot ?? ItemIdCollection.Empty).Select((id, i) => new HrtItem(id));
        PossibleItems = (possibleLoot ?? ItemIdCollection.Empty).Select((id, i) => new HrtItem(id));
    }
}