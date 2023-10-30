namespace HimbeertoniRaidTool.Common.Security;

public interface IIdProvider
{
    public uint GetAuthorityIdentifier();
    public HrtId CreateId(HrtId.IdType type);
    public bool SignId(HrtId id);
    public bool VerifySignature(HrtId id);
}