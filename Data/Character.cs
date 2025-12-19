using System.Collections;
using System.Security.Cryptography;
using HimbeertoniRaidTool.Common.Localization;
using HimbeertoniRaidTool.Common.Security;
using HimbeertoniRaidTool.Common.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Character : IEnumerable<PlayableClass>, IHrtDataTypeWithId<Character>, IFormattable, ICloneable<Character>
{
    #region Static

    private static readonly ExcelSheet<World> _worldSheet = CommonLibrary.ExcelModule.GetSheet<World>();

    private static readonly ExcelSheet<Tribe> _tribeSheet = CommonLibrary.ExcelModule.GetSheet<Tribe>();

    private static SHA256 _sha256 = SHA256.Create();

    public static string DataTypeName => CommonLoc.DataType_Character;

    public static HrtId.IdType IdType => HrtId.IdType.Character;

    public static Character Empty => new();

    public static ulong CalcCharId(long contentId) => CalcCharId((ulong)contentId);
    public static ulong CalcCharId(ulong contentId)
    {
        if (contentId == 0)
            return 0;
        byte[] hash = _sha256.ComputeHash(BitConverter.GetBytes(contentId));
        return BitConverter.ToUInt64(hash);
    }

    #endregion

    #region Serialized

    [JsonProperty("Classes")] private readonly List<PlayableClass> _classes = [];

    [JsonProperty("MainInventory")] public readonly Inventory MainInventory = new();

    /// <summary>
    ///     HRT specific unique ID used for local storage.
    /// </summary>
    [JsonProperty("LocalID", ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public HrtId LocalId { get; set; } = HrtId.Empty;

    /// <summary>
    ///     HRT specific unique IDs used for remote storage and lookup.
    /// </summary>
    [JsonProperty("RemoteIDs")] public readonly List<HrtId> RemoteIds = [];
    [JsonProperty("Wallet")] public readonly Wallet Wallet = new();

    /// <summary>
    ///     Character unique ID calculated from characters ContentID.
    /// </summary>
    [JsonProperty("HrtCharID")] public ulong CharId;
    [JsonProperty("Gender")] public Gender Gender = Gender.Unknown;

    //Identifiers
    [JsonProperty("WorldID")] public uint HomeWorldId;

    /// <summary>
    ///     Unique character identifier for Lodestone. Maps 1:1 to ContentID (but not calculable).
    /// </summary>
    [JsonProperty("LodestoneID")] public int LodestoneId;
    [JsonProperty("Name")] public string Name;
    [JsonProperty("Tribe")] public uint TribeId;
    // ReSharper disable once ReplaceWithFieldKeyword Does not work with Json serialization
    [JsonProperty("MainJob")] private Job? _mainJob;

    #endregion

    public Character() : this("", 0) { }

    [JsonConstructor]
    public Character(string name, uint worldId)
    {
        HomeWorldId = worldId;
        Name = name;
    }


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
    public Tribe Tribe => _tribeSheet.GetRow(TribeId);

    public World? HomeWorld
    {
        get => HomeWorldId > 0 ? _worldSheet.GetRow(HomeWorldId) : null;
        set => HomeWorldId = value?.RowId ?? 0;
    }

    public string Initials => string.Join(' ', Name.Split(" ").Select(word => word[0] + "."));
    public bool Filled => !LocalId.IsEmpty;

    public IEnumerable<PlayableClass> Classes => _classes;

    public PlayableClass? this[Job? type] => _classes.Find(x => x.Job == type);

    public IEnumerator<PlayableClass> GetEnumerator() => Classes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Classes.GetEnumerator();
    string IHrtDataType.Name => Name;

    IList<HrtId> IHasHrtId.RemoteIds => RemoteIds;

    public bool Equals(IHasHrtId? other) => LocalId.Equals(other?.LocalId);

    public PlayableClass AddClass(Job job)
    {
        PlayableClass classToAdd = new(job);
        _classes.Add(classToAdd);
        return classToAdd;
    }
    public override string ToString() => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider) => format switch
    {
        "a"  => "xxx @ xxx",
        "n"  => Name,
        "i"  => Initials,
        "is" => $"{Initials} @ {HomeWorld?.Name ?? CommonLoc.NotAvail_Abbrev}",
        _    => $"{Name} @ {HomeWorld?.Name ?? CommonLoc.NotAvail_Abbrev}",
    };


    public bool RemoveClass(Job type) => _classes.RemoveAll(job => job.Job == type) > 0;

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

    public Character Clone() => CloneService.Clone(this);


    // ReSharper disable NonReadonlyMemberInGetHashCode
    //TODO: Figure out proper way
    public override int GetHashCode() => LocalId.GetHashCode();

    public void MergeInfos(Character toMerge)
    {
        if (CharId == 0)
            CharId = toMerge.CharId;
        if (LodestoneId == 0)
            LodestoneId = toMerge.LodestoneId;
        if (TribeId == 0)
            TribeId = toMerge.TribeId;
        foreach (var job in _classes)
        {
            var jobToMerge = toMerge[job.Job];
            if (jobToMerge == null)
                continue;
            job.Level = Math.Max(job.Level, jobToMerge.Level);
            if (jobToMerge.CurGear.TimeStamp > job.CurGear.TimeStamp)
                job.CurGear.CopyFrom(jobToMerge.CurGear);
            if (jobToMerge.CurBis.TimeStamp > job.CurBis.TimeStamp)
                job.CurBis.CopyFrom(jobToMerge.CurBis);
            toMerge._classes.Remove(jobToMerge);
        }

        _classes.AddRange(toMerge._classes);
    }
}