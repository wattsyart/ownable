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
            BlockNumber = "12345",
            Id = id,
            Name = "My NFT",
            Symbol = "NFT",
            Type = "ERC721"
        };

        return contract;
    }
}