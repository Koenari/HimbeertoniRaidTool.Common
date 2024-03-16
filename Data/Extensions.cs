namespace HimbeertoniRaidTool.Common.Data;

public static class Extensions
{
    public static T Clone<T>(this T source) where T : ICloneable
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
}