using System;
using System.Globalization;
using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel;

namespace HimbeertoniRaidTool.Common.Services;
#pragma warning disable CS8618
public static class ServiceManager
{
    private static CuratedData _curatedData;
    public static ExcelModule ExcelModule { get; private set; }
    public static GameInfo GameInfo { get; private set; }
    public static ItemInfo ItemInfo { get; private set; }
    public static void Init(ExcelModule module, string? language = null)
    {
        ExcelModule = module;
        _curatedData = new CuratedData();
        GameInfo = new GameInfo(_curatedData);
        ItemInfo = new ItemInfo(module, _curatedData, GameInfo);
        if(language is not null)
            SetLanguage(language);
    }

    public static void SetLanguage(string langCode)
    {
        try
        {
            CommonLoc.Culture = new CultureInfo(langCode);
        }
        catch (Exception _) {}
    }
}
#pragma warning restore CS8618