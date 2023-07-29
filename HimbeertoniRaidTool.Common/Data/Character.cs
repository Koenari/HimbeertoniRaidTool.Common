﻿using HimbeertoniRaidTool.Common.Security;
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
public class Character : IEquatable<Character>, IEnumerable<PlayableClass>
{
    private static readonly ExcelSheet<World>? _worldSheet = ServiceManager.ExcelModule?.GetSheet<World>();

    private static readonly ExcelSheet<Tribe>? _tribeSheet = ServiceManager.ExcelModule?.GetSheet<Tribe>();

    //Identifiers
    [JsonProperty("WorldID")] public uint HomeWorldID;
    [JsonProperty("Name")] public string Name = "";

    /// <summary>
    /// Character unique ID calculated from characters ContentID.
    /// </summary>
    [JsonProperty("HrtCharID")] public ulong CharID = 0;

    /// <summary>
    /// Unique character identifier for Lodestone. Maps 1:1 to ContentID (but not calculatable).
    /// </summary>
    [JsonProperty("LodestoneID")] public int LodestoneID = 0;

    /// <summary>
    /// HRT specific uniuqe ID used for local storage.
    /// </summary>
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtID LocalID = HrtID.Empty;

    /// <summary>
    /// HRT specific uniuqe IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtID> RemoteIDs = new();

    //Properties
    [JsonProperty("Classes")] private readonly List<PlayableClass> _classes = new();
    [JsonProperty("MainJob")] private Job? _mainJob;
    [JsonProperty("Tribe")] public uint TribeID = 0;
    [JsonProperty("Gender")] public Gender Gender = Gender.Unknown;
    [JsonProperty("Wallet")] public readonly Wallet Wallet = new();

    [JsonProperty("MainInventory")] public readonly Inventory MainInventory = new();

    //Runtime only Properties
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
    public Tribe? Tribe => _tribeSheet?.GetRow(TribeID);

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

    public bool Equals(Character? other)
    {
        if (other == null)
            return false;
        return Name.Equals(other.Name) && HomeWorldID == other.HomeWorldID;
    }

    public override bool Equals(object? obj)
    {
        return obj is Character objS && Equals(objS);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static ulong CalcCharID(long contentID)
    {
        if (contentID == 0)
            return 0;
        using var sha256 = SHA256.Create();
#pragma warning disable CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        //Static version crashes in wine
        byte[] hash = sha256.ComputeHash(BitConverter.GetBytes(contentID));
#pragma warning restore CA1850 // Prefer static 'HashData' method over 'ComputeHash'
        return BitConverter.ToUInt64(hash);
    }

    public IEnumerator<PlayableClass> GetEnumerator()
    {
        return Classes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Classes.GetEnumerator();
    }

    public void MergeInfos(Character toMerge)
    {
        if (CharID == 0)
            CharID = toMerge.CharID;
        if (LodestoneID == 0)
            LodestoneID = toMerge.LodestoneID;
        if (TribeID == 0)
            TribeID = toMerge.TribeID;
        foreach (PlayableClass job in _classes)
        {
            PlayableClass? jobToMerge = toMerge[job.Job];
            if (jobToMerge == null)
                continue;
            job.Level = Math.Max(job.Level, jobToMerge.Level);
            if (jobToMerge.Gear.TimeStamp > job.Gear.TimeStamp)
                job.Gear.CopyFrom(jobToMerge.Gear);
            if (jobToMerge.BIS.TimeStamp > job.BIS.TimeStamp)
                job.BIS.CopyFrom(jobToMerge.BIS);
            toMerge._classes.Remove(jobToMerge);
        }

        _classes.AddRange(toMerge._classes);
    }
}