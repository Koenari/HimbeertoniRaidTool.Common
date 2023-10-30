namespace HimbeertoniRaidTool.Common.Security;

public interface IIDProvider
{
    public uint GetAuthorityIdentifier();
    public HrtId CreateID(HrtId.IdType type);
    public bool SignID(HrtId id);
    public bool VerifySignature(HrtId id);
}