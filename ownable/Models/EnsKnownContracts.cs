namespace ownable.Models;

internal sealed class EnsKnownContracts : IKnownContracts
{
    private readonly Dictionary<string, Contract?> _registry;

    public EnsKnownContracts()
    {
        var contract = new Contract
        {
            Address = "0x57f1887a8BF19b14fC0dF6Fd9B2acc9Af147eA85",
            Type = "ERC721",
            Name = "Ethereum Name Service",
            Symbol = "ENS"
        };

        _registry = new Dictionary<string, Contract?>(StringComparer.OrdinalIgnoreCase) {{ contract.Address, contract }};
    }

    public bool TryGetContract(string contractAddress, out Contract? contract) => _registry.TryGetValue(contractAddress, out contract);
}