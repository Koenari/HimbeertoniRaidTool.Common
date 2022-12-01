using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HimbeertoniRaidTool.Common.Data;

public class InstanceWithLoot
{
    private static readonly ExcelSheet<InstanceContent>? InstanceSheet = ServiceManager.ExcelModule.GetSheet<InstanceContent>();
    private static readonly ExcelSheet<ContentFinderCondition>? ContentFinderSheet = ServiceManager.ExcelModule.GetSheet<ContentFinderCondition>();
    private static readonly Dictionary<uint, uint> ContenFinderLookup;
    static InstanceWithLoot()
    {
        ContenFinderLookup = new Dictionary<uint, uint>();
        if (ContentFinderSheet != null)
            foreach (var row in ContentFinderSheet.Where(x => x.ContentLinkType == 1))
                ContenFinderLookup.TryAdd(row.Content, row.RowId);
    }
    public InstanceType InstanceType => (InstanceType)InstanceContent.InstanceContentType;

    public EncounterDifficulty Difficulty { get; }
    public string Name => ContentFinderCondition.Name;
    public IEnumerable<HrtItem> PossibleItems { get; }
    public IEnumerable<HrtItem> GuaranteedItems { get; }
    public uint InstanceID => InstanceContent.RowId;
    private readonly InstanceContent InstanceContent;
    private readonly ContentFinderCondition ContentFinderCondition;
    public IEnumerable<HrtItem> AllLoot
    {
        get
        {
            foreach (var item in PossibleItems)
                yield return item;
            foreach (var item in GuaranteedItems)
                yield return item;
        }
    }
    public InstanceWithLoot(uint id, EncounterDifficulty difficulty = EncounterDifficulty.Normal, ItemIDCollection? possibleLoot = null, ItemIDCollection? guaranteedLoot = null)
    {
        InstanceContent = InstanceSheet?.GetRow(id) ?? new();
        ContentFinderCondition = ContenFinderLookup.TryGetValue(id, out uint contentID) ? ContentFinderSheet!.GetRow(contentID)! : new();
        Difficulty = difficulty;
        GuaranteedItems = (guaranteedLoot ?? ItemIDCollection.Empty).Select((id, i) => new HrtItem(id));
        PossibleItems = (possibleLoot ?? ItemIDCollection.Empty).Select((id, i) => new HrtItem(id));
    }
}
