using System.Globalization;
using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel;

namespace HimbeertoniRaidTool.Common.Services;
#pragma warning disable CS8618
public static class ServiceManager
{
    private static bool _isInitialized;
    internal static ExcelModule ExcelModule { get; private set; }
    internal static ItemInfoService ItemInfoService { get; private set; }
    public static void Init(ExcelModule module, string? language = null)
    {
        if (_isInitialized) return;
        _isInitialized = true;
        ExcelModule = module;
        ItemInfoService = new ItemInfoService(module);
        if (language is not null)
            SetLanguage(language);
    }

    public static void SetLanguage(string langCode) => CommonLoc.Culture = new CultureInfo(langCode);
}
#pragma warning restore CS8618