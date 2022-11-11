namespace ownable.Models;

[Flags]
public enum IndexScope
{
    None = 0x0,
    TokenTransfers = 0x1,
    TokenContracts = 0x2,
    TokenMetadata = 0x4,
    TokenMetadataAttributes = 0x8,

    All = int.MaxValue
}

public static class IndexScopeExtensions
{
    public static bool HasFlagFast(this IndexScope value, IndexScope flag)
    {
        return (value & flag) != 0;
    }
}