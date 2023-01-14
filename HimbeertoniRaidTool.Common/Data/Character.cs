using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Character : IEquatable<Character>, IEnumerable<PlayableClass>
{
    private static readonly ExcelSheet<World>? _worldSheet = ServiceManager.ExcelModule?.GetSheet<World>();
    private static readonly ExcelSheet<Tribe>? _tribeSheet = ServiceManager.ExcelModule?.GetSheet<Tribe>();
    [JsonProperty("Classes")]
    private readonly List<PlayableClass> _classes = new();
    [JsonProperty("Name")]
    public string Name = "";
    [JsonProperty("MainJob")]
    private Job? _mainJob;
    public Job? MainJob
    {
        get
        {
            if (_mainJob == null && Classes.Any())
                _mainJob = Classes.First().Job;
            return _mainJob;
        }
        set => _mainJob = value;
    }
    public PlayableClass? MainClass => MainJob.HasValue ? this[MainJob.Value] : null;
    [JsonProperty("WorldID")]
    public uint HomeWorldID;
    [JsonProperty("Tribe")]
    public uint TribeID = 0;
    [JsonProperty]
    public Gender Gender = Gender.Unknown;
    [JsonIgnore]
    public Tribe? Tribe => _tribeSheet?.GetRow(TribeID)!;
    /// <summary>
    /// Unique identifier for characters. Can only be read from withing game client.
    /// </summary>
    [JsonProperty("ContentID")]
    public long ContentID = 0;
    /// <summary>
    /// Unique character identifier for Lodestone. Maps 1:1 to Content (but not hte same.
    /// </summary>
    [JsonProperty("LodestoneID")]
    public int LodestoneID = 0;
    [JsonProperty("Wallet")]
    public readonly Wallet Wallet = new();
    [JsonProperty("MainInventory")]
    public readonly Inventory MainInventory = new();
    public World? HomeWorld
    {
        get => HomeWorldID > 0 ? _worldSheet?.GetRow(HomeWorldID) : null;
        set => HomeWorldID = value?.RowId ?? 0;
    }
    public bool Filled => Name != "";
    public Character(string name = "", uint worldID = 0)
    {
        Name = name;
        HomeWorldID = worldID;
    }
    public IEnumerable<PlayableClass> Classes => _classes;
    public PlayableClass AddClass(Job job)
    {
        PlayableClass classtoAdd = new(job, this);
        _classes.Add(classtoAdd);
        return classtoAdd;
    }
    public PlayableClass? this[Job type] => _classes.Find(x => x.Job == type);
    public bool RemoveClass(Job type)
    {
        return _classes.RemoveAll(job => job.Job == type) > 0;
    }
    public bool Equals(Character? other)
    {
        if (other == null)
            return false;
        return Name.Equals(other.Name) && HomeWorldID == other.HomeWorldID;
    }
    public override bool Equals(object? obj) => obj is Character objS && Equals(objS);
    public override int GetHashCode() => Name.GetHashCode();

    public IEnumerator<PlayableClass> GetEnumerator() => Classes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Classes.GetEnumerator();

}
