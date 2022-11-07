namespace ownable.Indexers;

[Flags]
public enum ContractFeatures
{
    None = 0x0,
    SupportsStandard = 0x1,
    SupportsMetadata = 0x2,
    SupportsUri = 0x4,
    SupportsName = 0x8,
    SupportsSymbol = 0x10,
}

public static class ContractFeaturesExtensions
{
    public static bool SupportsStandard(this ContractFeatures value) => value.HasFlagFast(ContractFeatures.SupportsStandard);
    public static bool SupportsMetadata(this ContractFeatures value) => value.HasFlagFast(ContractFeatures.SupportsMetadata);
    public static bool SupportsUri(this ContractFeatures value) => value.HasFlagFast(ContractFeatures.SupportsUri);
    public static bool SupportsName(this ContractFeatures value) => value.HasFlagFast(ContractFeatures.SupportsName);
    public static bool SupportsSymbol(this ContractFeatures value) => value.HasFlagFast(ContractFeatures.SupportsSymbol);
    
    public static bool HasFlagFast(this ContractFeatures value, ContractFeatures flag)
    {
        return (value & flag) != 0;
    }
}