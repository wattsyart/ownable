
using ownable.Models.Indexed;

namespace ownable.tests
{
    public class IndexingTests
    {
        [Fact]
        public void IndexAndRetrieveObjects()
        {
            var store = new Store($"test-{Guid.NewGuid()}");

            try
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

                store.Index(contract);

                var contracts = store.Get<Contract>(CancellationToken.None).ToList();
                Assert.NotNull(contracts);
                Assert.Single(contracts);
                
                var retrieved = contracts.Single();
                Assert.Equal(contract.Address, retrieved.Address);
                Assert.Equal(contract.BlockNumber, retrieved.BlockNumber);
                Assert.Equal(contract.Id, retrieved.Id);
                Assert.Equal(contract.Name, retrieved.Name);
                Assert.Equal(contract.Symbol, retrieved.Symbol);
                Assert.Equal(contract.Type, retrieved.Type);

            }
            finally
            {
                store.Dispose();
            }
        }
    }
}