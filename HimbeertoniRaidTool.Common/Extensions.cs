using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common;

public static class Extensions
{
    public static T Clone<T>(this T source)
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
    [Obsolete()]
    public static int ConsistentHash(this string obj)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] sha = sha256.ComputeHash(Encoding.UTF8.GetBytes(obj));
        return sha[0] + 256 * sha[1] + 256 * 256 * sha[2] + 256 * 256 * 256 * sha[2];
    }
}

