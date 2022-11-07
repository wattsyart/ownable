using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.tests
{
    public class IndexingTests
    {
        [Fact]
        public void LookupScalability()
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path);

            var keys = new List<byte[]>();
            for (var i = 0; i < 1000; i++)
            {
                var contract = TestFactory.GetContract();
                contract.Name = Guid.NewGuid().ToString();
                store.Append(contract);
                keys.Add(KeyBuilder.LookupKey(typeof(Contract), nameof(Contract.Name), contract.Name));
            }

            foreach (var key in keys)
            {
                var results = store.FindByKey<Contract>(key, CancellationToken.None);
                Assert.NotNull(results);
                Assert.Single(results);
            }
        }

        [Fact]
        public void AppendObjectAndLookupWithIndexedKeys()
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path);

            var startLength = store.GetEntriesCount();

            try
            {
                var contract = TestFactory.GetContract();
                store.Append(contract);

                var indexLength = store.GetEntriesCount();
                Assert.True(startLength < indexLength);

                var cancellationToken = CancellationToken.None;

                var contractById = store.GetById<Contract>(contract.Id, cancellationToken);
                Assert.NotNull(contractById);
                AssertEqual(contract, contractById!);
                
                {
                    var contractWithAddress = store.Find<Contract>(nameof(Contract.Address), contract.Address, cancellationToken).ToList();
                    Assert.Single(contractWithAddress);
                    AssertEqual(contract, contractWithAddress.Single());

                    var contractWithBlockNumber = store.Find<Contract>(nameof(Contract.BlockNumber), contract.BlockNumber, cancellationToken).ToList();
                    Assert.Single(contractWithBlockNumber);
                    AssertEqual(contract, contractWithBlockNumber.Single());

                    var contractWithName = store.Find<Contract>(nameof(Contract.Name), contract.Name, cancellationToken).ToList();
                    Assert.Single(contractWithName);
                    AssertEqual(contract, contractWithName.Single());

                    var contractWithSymbol = store.Find<Contract>(nameof(Contract.Symbol), contract.Symbol, cancellationToken).ToList();
                    Assert.Single(contractWithSymbol);
                    AssertEqual(contract, contractWithSymbol.Single());

                    var contractWithType = store.Find<Contract>(nameof(Contract.Type), contract.Type, cancellationToken).ToList();
                    Assert.Single(contractWithType);
                    AssertEqual(contract, contractWithType.Single());
                }
            }
            finally
            {
                store.Dispose();
            }
        }

        [Fact]
        public void LegacyIndexAndRetrieveObjects()
        {
            var store = new Store($"test-{Guid.NewGuid()}");
            var startLength = store.GetEntriesCount();

            try
            {
                var contract = TestFactory.GetContract();
                store.Save(contract);

                var indexLength = store.GetEntriesCount();
                Assert.True(startLength < indexLength);

                var contracts = store.Get<Contract>(CancellationToken.None).ToList();
                Assert.NotNull(contracts);
                Assert.Single(contracts);
                
                var retrieved = contracts.Single();
                AssertEqual(contract, retrieved);
            }
            finally
            {
                store.Dispose();
            }
        }

        private static void AssertEqual(Contract expected, Contract actual)
        {
            Assert.Equal(expected.Address, actual.Address);
            Assert.Equal(expected.BlockNumber, actual.BlockNumber);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Symbol, actual.Symbol);
            Assert.Equal(expected.Type, actual.Type);
        }
    }
}