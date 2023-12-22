using Lumina.Excel;

namespace HimbeertoniRaidTool.Common.Services;
#pragma warning disable CS8618
public static class ServiceManager
{
    private static CuratedData _curatedData;
    public static ExcelModule ExcelModule { get; private set; }
    public static GameInfo GameInfo { get; private set; }
    public static ItemInfo ItemInfo { get; private set; }
    public static void Init(ExcelModule module)
    {
        ExcelModule = module;
        _curatedData = new CuratedData();
        GameInfo = new GameInfo(_curatedData);
        ItemInfo = new ItemInfo(module, _curatedData, GameInfo);
    }
}
#pragma warning restore CS8618