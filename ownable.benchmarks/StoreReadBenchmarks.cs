using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.benchmarks;

[MemoryDiagnoser]
public class StoreReadBenchmarks
{
    private Store? _store;
    private readonly List<Contract> _items = new();
    private readonly List<byte[]> _keys = new();

    [Params(50, 100, 1000)]
    public int Count { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _store?.Dispose();
        _store = new Store($"benchmark-{Guid.NewGuid()}", NullLogger<Store>.Instance);
        for (var i = 0; i < Count; i++)
        {
            var item = GetContract();
            _items.Add(item);
            _keys.Add(KeyBuilder.LookupKey(typeof(Contract), nameof(Contract.Name), item.Name).ToArray());
            _store.Append(item, CancellationToken.None);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _store?.Dispose();
        _items.Clear();
        _keys.Clear();
    }
    
    [Benchmark(Baseline = true)]
    public void GetById()
    {
        foreach (var item in _items)
            _store?.GetById<Contract>(item.Id, CancellationToken.None);
    }

    [Benchmark]
    public void FindByKey()
    {
        foreach (var key in _keys)
            _store?.FindByKey<Contract>(key, CancellationToken.None);
    }

    public static Contract GetContract()
    {
        var contract = new Contract
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = "12345",
            Name = $"{Guid.NewGuid()}",
            Symbol = "NFT",
            Type = "ERC721"
        };

        return contract;
    }
}