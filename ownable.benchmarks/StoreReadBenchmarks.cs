using BenchmarkDotNet.Attributes;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.benchmarks;

[MemoryDiagnoser]
public class StoreReadBenchmarks
{
    private Store _store = null!;
    private readonly List<Contract> _items = new();

    [Params(50, 100, 1000)]
    public int Count { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _store.Dispose();
        _store = new Store($"benchmark-{Guid.NewGuid()}");
        _items.Clear();
        for (var i = 0; i < Count; i++)
        {
            var item = GetContract();
            _items.Add(item);
            _store.Append(item);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _store.Dispose();
    }
    
    [Benchmark(Baseline = true)]
    public void GetById()
    {
        foreach (var item in _items)
            _store.GetById<Contract>(item.Id, CancellationToken.None);
    }

    [Benchmark]
    public void FindByKey()
    {
        foreach (var item in _items)
            _store.Find<Contract>(nameof(Contract.Name), item.Name, CancellationToken.None);
    }

    public static Contract GetContract()
    {
        var contract = new Contract
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = "12345",
            Name = $"{Guid.NewGuid}",
            Symbol = "NFT",
            Type = "ERC721"
        };

        return contract;
    }
}