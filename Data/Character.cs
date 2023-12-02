using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Character : IEnumerable<PlayableClass>, IHasHrtId
{
    private static readonly ExcelSheet<World>? _worldSheet = ServiceManager.ExcelModule.GetSheet<World>();

    private static readonly ExcelSheet<Tribe>? _tribeSheet = ServiceManager.ExcelModule.GetSheet<Tribe>();

    [JsonIgnore] public HrtId.IdType IdType => HrtId.IdType.Character;
    //Identifiers
    [JsonProperty("WorldID")] public uint HomeWorldId;
    [JsonProperty("Name")] public string Name;

    /// <summary>
    /// Character unique ID calculated from characters ContentID.
    /// </summary>
    [JsonProperty("HrtCharID")] public ulong CharId = 0;

    /// <summary>
    /// Unique character identifier for Lodestone. Maps 1:1 to ContentID (but not calculable).
    /// </summary>
    [JsonProperty("LodestoneID")] public int LodestoneId = 0;

    /// <summary>
    /// HRT specific unique ID used for local storage.
    /// </summary>
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    /// HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = new();
    [JsonIgnore] IEnumerable<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    //Properties
    [JsonProperty("Classes")] private readonly List<PlayableClass> _classes = new();
    [JsonProperty("MainJob")] private Job? _mainJob;
    [JsonProperty("Tribe")] public uint TribeId = 0;
    [JsonProperty("Gender")] public Gender Gender = Gender.Unknown;
    [JsonProperty("Wallet")] public readonly Wallet Wallet = new();

    [JsonProperty("MainInventory")] public readonly Inventory MainInventory = new();

    //Runtime only Properties
    public Job? MainJob
    {
        get
        {
            if (_classes.All(c => c.Job != _mainJob))
                _mainJob = null;
            _mainJob ??= _classes.FirstOrDefault()?.Job;
            return _mainJob;
        }
        set => _mainJob = value;
    }

    public PlayableClass? MainClass => this[MainJob];
    public Tribe? Tribe => _tribeSheet?.GetRow(TribeId);

    public World? HomeWorld
    {
        get => HomeWorldId > 0 ? _worldSheet?.GetRow(HomeWorldId) : null;
        set => HomeWorldId = value?.RowId ?? 0;
    }

    public bool Filled => !LocalId.IsEmpty;

    public Character(string name = "", uint worldId = 0)
    {
        Name = name;
        HomeWorldId = worldId;
    }

    public IEnumerable<PlayableClass> Classes => _classes;

    public PlayableClass AddClass(Job job)
    {
        PlayableClass classToAdd = new(job, this);
        _classes.Add(classToAdd);
        return classToAdd;
    }

    public PlayableClass? this[Job? type] => _classes.Find(x => x.Job == type);

    public bool RemoveClass(Job type)
    {
        return _classes.RemoveAll(job => job.Job == type) > 0;
    }

    public bool CanMoveUp(PlayableClass c)
    {
        int idx = _classes.IndexOf(c);
        return idx > 0;
    }

    public void MoveClassUp(PlayableClass c)
    {
        if (!CanMoveUp(c)) return;
        int idx = _classes.IndexOf(c);
        (_classes[idx - 1], _classes[idx]) = (_classes[idx], _classes[idx - 1]);
    }

    public bool CanMoveDown(PlayableClass c)
    {
        int idx = _classes.IndexOf(c);
        return idx < _classes.Count - 1;
    }

    public void MoveClassDown(PlayableClass c)
    {
        if (!CanMoveDown(c)) return;
        int idx = _classes.IndexOf(c);
        (_classes[idx], _classes[idx + 1]) = (_classes[idx + 1], _classes[idx]);
    }

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);

    public override bool Equals(object? obj) => obj is Character objS && Equals(objS);

    public override int GetHashCode() => LocalId.GetHashCode();

    public static ulong CalcCharId(long contentId)
    {
        if (contentId == 0)
            return 0;
        using var sha256 = SHA256.Create();
#pragma warning disable CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        //Static version crashes in wine
        byte[] hash = sha256.ComputeHash(BitConverter.GetBytes(contentId));
#pragma warning restore CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        return BitConverter.ToUInt64(hash);
    }

    public IEnumerator<PlayableClass> GetEnumerator() => Classes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Classes.GetEnumerator();

    public void MergeInfos(Character toMerge)
    {
        if (CharId == 0)
            CharId = toMerge.CharId;
        if (LodestoneId == 0)
            LodestoneId = toMerge.LodestoneId;
        if (TribeId == 0)
            TribeId = toMerge.TribeId;
        foreach (PlayableClass job in _classes)
        {
            PlayableClass? jobToMerge = toMerge[job.Job];
            if (jobToMerge == null)
                continue;
            job.Level = Math.Max(job.Level, jobToMerge.Level);
            if (jobToMerge.Gear.TimeStamp > job.Gear.TimeStamp)
                job.Gear.CopyFrom(jobToMerge.Gear);
            if (jobToMerge.Bis.TimeStamp > job.Bis.TimeStamp)
                job.Bis.CopyFrom(jobToMerge.Bis);
            toMerge._classes.Remove(jobToMerge);
        }

        _classes.AddRange(toMerge._classes);
    }
}