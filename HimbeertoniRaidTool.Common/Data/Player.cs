using System.Collections.Generic;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Player
{
    [JsonProperty("NickName")]
    public string NickName = "";
    [JsonProperty("AdditionalData", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public readonly AdditionalPlayerData AdditionalData = new();
    [JsonProperty("Chars")]
    public List<Character> Chars { get; set; } = new();
    public bool Filled => NickName != "" || Chars.Count > 0 && Chars[0].Filled;
    public Character MainChar
    {
        get
        {
            if (Chars.Count == 0)
                Chars.Insert(0, new());
            return Chars[0];
        }
        set
        {
            Chars.Remove(value);
            Chars.Insert(0, value);
        }
    }
    public PlayableClass? CurJob => MainChar.MainClass;
    [JsonConstructor]
    public Player() { }
    public void Reset()
    {
        NickName = "";
        Chars.Clear();
    }
}
[JsonObject(MemberSerialization.OptIn)]
public class AdditionalPlayerData
{
    private const string ManualDPSKey = "ManualDPS";
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, int> IntData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, float> FloatData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, string> StringData = new();

    public int ManualDPS
    {
        get { return IntData.GetValueOrDefault(ManualDPSKey, 0); }
        set { IntData[ManualDPSKey] = value; }
    }
}
