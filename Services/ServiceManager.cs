using Lumina.Excel;

namespace HimbeertoniRaidTool.Common.Services;
#pragma warning disable CS8618
public static class ServiceManager
{
    private static ExcelModule _module;
    private static CuratedData _curatedData;
    private static GameInfo _gameInfo;
    private static ItemInfo _itemInfo;
    public static ExcelModule ExcelModule { get => _module; }
    public static GameInfo GameInfo => _gameInfo;
    public static ItemInfo ItemInfo => _itemInfo;
    public static void Init(ExcelModule module)
    {
        _module = module;
        _curatedData = new CuratedData();
        _gameInfo = new(_curatedData);
        _itemInfo = new ItemInfo(module, _curatedData, _gameInfo);
    }
}
#pragma warning restore CS8618
