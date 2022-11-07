namespace ownable.Models;

internal sealed class KnownContracts : IKnownContracts
{
    private readonly Dictionary<string, Contract?> _registry = new(StringComparer.OrdinalIgnoreCase);

    public KnownContracts()
    {
        AddContract(new Contract
        {
            Address = "0x57f1887a8BF19b14fC0dF6Fd9B2acc9Af147eA85",
            Type = "ERC721",
            Name = "Ethereum Name Service",
            Symbol = "ENS"
        });

        AddContract(new Contract
        {
            Address = "0x2e12d051c3Be2aAA932EB09CE4Cba8F58fa1860e",
            Type = "ERC721",
            Name = "Nouns Name Service",
            Symbol = "NNS"
        });
    }

    private void AddContract(Contract contract)
    {
        _registry[contract.Address] = contract;
    }

    public bool TryGetContract(string contractAddress, out Contract? contract) => _registry.TryGetValue(contractAddress, out contract);
}