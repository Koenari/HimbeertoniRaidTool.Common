using HimbeertoniRaidTool.Common.Data;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common;

public static class Extensions
{
    public static T Clone<T>(this T source) where T : ICloneable
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
}