using Newtonsoft.Json;
using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Player : IHasHrtId
{
    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Player;

    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();

    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;

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
                Chars.Insert(0, new Character());
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
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, int> IntData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, float> FloatData = new();
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public Dictionary<string, string> StringData = new();

}