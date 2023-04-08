using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common;

public static class Extensions
{
    public static T Clone<T>(this T source)
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
}

