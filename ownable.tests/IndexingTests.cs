using Microsoft.Extensions.Logging.Abstractions;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.tests
{
    public class IndexingTests
    {
        [Fact]
        public void AppendVsSaveWithSameKey()
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path, NullLogger<Store>.Instance);

            var contract = TestFactory.GetContract();
            contract.BlockNumber = 1000;

            store.Append(contract, CancellationToken.None);

            contract.BlockNumber = 1001;
            store.Append(contract, CancellationToken.None);

            var all = store.Get<Contract>(CancellationToken.None).ToList();
            Assert.Single(all);

            // since we called append, no change is registered
            Assert.Equal(1000, all.Single().BlockNumber);

            // calling save, allows mutation, so block number is changed
            store.Save(contract, CancellationToken.None);
            all = store.Get<Contract>(CancellationToken.None).ToList();
            Assert.Single(all);
            AssertEqual(contract, all.Single());
        }

        [Fact]
        public void BlockNumberFilter()
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path, NullLogger<Store>.Instance);

            var contract = TestFactory.GetContract();
            contract.Id = Guid.NewGuid();
            contract.BlockNumber = 1000;

            store.Append(contract, CancellationToken.None);

            contract.Id = Guid.NewGuid();
            contract.BlockNumber = 1001;
            store.Append(contract, CancellationToken.None);

            // getting all contracts by this address gives two, because we have two distinct IDs (this should switch to using a composite key including BlockNumber)
            var contracts = store.Find<Contract>(nameof(Contract.Address), contract.Address, CancellationToken.None).ToList();
            Assert.Equal(2, contracts.Count);

            // filtering by block number (only blocks > 1000) should produce a single result
            contracts = store.FindSince<Contract>(nameof(Contract.Address), contract.Address, 1000, CancellationToken.None).ToList();
            Assert.Single(contracts);
        }

        [Theory]
        [InlineData(1000)]
        public void LookupScalability(int records)
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path, NullLogger<Store>.Instance);

            var added = new List<Contract>();
            var keys = new List<byte[]>();
            for (var i = 0; i < records; i++)
            {
                var contract = TestFactory.GetContract();
                contract.Name = Guid.NewGuid().ToString();

                added.Add(contract);
                store.Append(contract, CancellationToken.None);
                keys.Add(KeyBuilder.KeyLookup(typeof(Contract), nameof(Contract.Name), contract.Name).ToArray());
            }

            foreach (var key in keys)
            {
                var results = store.FindByKey<Contract>(key, CancellationToken.None);
                Assert.NotNull(results);
                Assert.Single(results);
            }

            var all = store.Get<Contract>(CancellationToken.None).ToList();
            Assert.Equal(records, all.Count);

            added = added.OrderBy(x => x.Id).ToList();
            all = all.OrderBy(x => x.Id).ToList();

            for (var i = 0; i < all.Count; i++)
                AssertEqual(added[i], all[i]);
        }

        [Fact]
        public void AppendObjectAndLookupWithIndexedKeys()
        {
            var path = $"test-{Guid.NewGuid()}";
            var store = new Store(path, NullLogger<Store>.Instance);

            var startLength = store.GetEntriesCount();

            try
            {
                var contract = TestFactory.GetContract();
                store.Append(contract, CancellationToken.None);

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

                    var contractWithBlockNumber = store.Find<Contract>(nameof(Contract.BlockNumber), contract.BlockNumber.ToString(), cancellationToken).ToList();
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