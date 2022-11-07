using BenchmarkDotNet.Attributes;
using ownable.Data;
using ownable.Models.Indexed;

namespace ownable.benchmarks;

[MemoryDiagnoser]
public class StoreBenchmarks
{
    private Store _store = null!;
    private readonly List<Indexable> _items = new();

    [Params(50, 100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _store = new Store($"benchmark-{Guid.NewGuid()}");
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _items.Clear();
        for (var i = 0; i < Count; i++)
            _items.Add(GetContract());
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _store.Dispose();
    }

    [Benchmark]
    public void Append()
    {
        foreach(var item in _items)
            _store.Append(item);
    }

    public static Contract GetContract()
    {
        var contract = new Contract
        {
            Address = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb",
            BlockNumber = "12345",
            Name = "My NFT",
            Symbol = "NFT",
            Type = "ERC721"
        };

        return contract;
    }
}