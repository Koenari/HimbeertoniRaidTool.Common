using ICloneable = HimbeertoniRaidTool.Common.Data.ICloneable;

namespace HimbeertoniRaidTool.Common.Extensions;

public static class DataExtensions
{
    public static T Clone<T>(this T source) where T : ICloneable
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
}