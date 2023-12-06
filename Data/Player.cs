﻿using System;
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
    private readonly List<Character> _characters = new();
    [JsonProperty("MainCharIndex")]
    private int _mainCharIndex = 0;
    public IEnumerable<Character> Characters => _characters;

    public bool Filled => !LocalId.IsEmpty;
    public Character MainChar
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            // Deserialization Problems
            _characters.RemoveAll(c => c is null);
            if (_characters.Count == 0)
                _characters.Add(new Character());
            _mainCharIndex = Math.Clamp(_mainCharIndex, 0, _characters.Count - 1);
            return _characters[_mainCharIndex];
        }
        set
        {
            if (value.Equals(_characters[_mainCharIndex])) return;
            _mainCharIndex = _characters.FindIndex(c => c.Equals(value));
            if (_mainCharIndex >= 0)
                return;
            _characters.Add(value);
            _mainCharIndex = _characters.Count - 1;
        }
    }
    public PlayableClass? CurJob => MainChar.MainClass;
    [JsonConstructor]
    public Player() { }


    public void RemoveCharacter(Character character)
    {
        Character mainChar = MainChar;
        _characters.Remove(character);
        MainChar = mainChar;
    }
    public void AddCharacter(Character character) => _characters.Add(character);
    public bool Equals(IHasHrtId? obj) => LocalId.Equals(obj?.LocalId);
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