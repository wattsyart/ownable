using ownable.Models.Indexed;

namespace ownable.tests;

public static class TestFactory
{
    public static Contract GetContract()
    {
        var id = Guid.NewGuid();

        var contract = new Contract
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = 12345,
            Id = id,
            Name = "My NFT",
            Symbol = "NFT",
            Type = "ERC721"
        };

        return contract;
    }

    public static Sent GetSent()
    {
        var id = Guid.NewGuid();

        var sent = new Sent
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = 12345,
            ContractAddress = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            Id = id,
            TokenId = 12345
        };

        return sent;
    }

    public static Received GetReceived()
    {
        var id = Guid.NewGuid();

        var sent = new Received
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = 12345,
            ContractAddress = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            Id = id,
            TokenId = 12345
        };

        return sent;
    }

    public static List<Trait> GetTraits()
    {
        var id = Guid.NewGuid();

        var trait = new Trait
        {
            Id = id,
            BlockNumber = 12345,
            Type = "Mood",
            Value = "Optimistic"
        };

        var traits = new List<Trait> {trait};
        return traits;
    }
}