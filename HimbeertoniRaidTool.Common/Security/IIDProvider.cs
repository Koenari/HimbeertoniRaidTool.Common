namespace HimbeertoniRaidTool.Common.Security;
public interface IIDProvider
{
    public uint GetAuthorityIdentifier();
    public HrtID CreateID(HrtID.IDType type);
    public bool SignID(HrtID id);
    public bool VerifySignature(HrtID id);
}
