using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common;

public static class Extensions
{
    public static T Clone<T>(this T source)
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source))!;
    public static int ConsistentHash(this string obj)
    {
        SHA512 alg = SHA512.Create();
        byte[] sha = alg.ComputeHash(Encoding.UTF8.GetBytes(obj));
        return sha[0] + 256 * sha[1] + 256 * 256 * sha[2] + 256 * 256 * 256 * sha[2];
    }
}

