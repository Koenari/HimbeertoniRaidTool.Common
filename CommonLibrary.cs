using System.Globalization;
using HimbeertoniRaidTool.Common.Extensions;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;

namespace HimbeertoniRaidTool.Common;
#pragma warning disable CS8618
public static class CommonLibrary
{
    private static bool _isInitialized;
    internal static ExcelModule ExcelModule
    {
        get => field ?? throw new InvalidOperationException("CommonLibrary is not initialized.");
        private set;
    }
    public static void Init(ExcelModule module, string? language = null)
    {
        if (_isInitialized) return;
        _isInitialized = true;
        ExcelModule = module;
        ItemExtensions.ItemInfoService = new ItemInfoService(module);
        if (language is not null)
            SetLanguage(language);
    }

    public static void SetLanguage(string langCode) => SetLanguage(new CultureInfo(langCode));
    public static void SetLanguage(CultureInfo culture) => CommonLoc.Culture = culture;

}


#pragma warning restore CS8618