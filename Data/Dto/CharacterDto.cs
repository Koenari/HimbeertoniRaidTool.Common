using HimbeertoniRaidTool.Common.Security;

namespace HimbeertoniRaidTool.Common.Data.Dto;

public class CharacterDto
{
    public CharacterDto(Character character)
    {
        Wallet = character.Wallet.ToDto();
        MainJob = character.MainJob;
        CharId = character.CharId;
        Gender = character.Gender;
        HomeWorldId = character.HomeWorldId;
        LodestoneId = character.LodestoneId;
        Name = character.Name;
        TribeId = character.TribeId;
    }

    public readonly WalletDto Wallet;

    public Job? MainJob;

    public ulong CharId;

    public Gender Gender;

    public uint HomeWorldId;

    public int LodestoneId;
    public string Name;
    public uint TribeId;

}

public sealed class ClassDto
{
    private readonly List<HrtId> BisSets = [];

    private readonly List<HrtId> GearSets = [];

    public Job Job;

    public int Level;
    public ClassDto(PlayableClass job)
    {
        Job = job.Job;
        Level = job.Level;
        foreach (var set in job.GearSets)
        {
            GearSets.Add(set.LocalId);
        }
        foreach (var set in job.BisSets)
        {
            BisSets.Add(set.LocalId);
        }
    }

}

public sealed class WalletDto
{
    public WalletDto(Wallet wallet)
    {
        LastUpdated = wallet.LastUpdated;
        foreach ((var key, int value) in wallet)
        {
            Data.Add(key, value);
        }
    }
    public readonly Dictionary<Currency, int> Data = [];
    public DateTime LastUpdated;
}