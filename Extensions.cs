namespace HimbeertoniRaidTool.Common;

public static class Extensions
{
    public static T Clone<T>(this T source) where T : Data.ICloneable
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
}