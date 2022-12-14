using BenchmarkDotNet.Running;

namespace ownable.benchmarks;

public static class Program
{
    public static void Main(params string[] args)
    {
        BenchmarkRunner.Run<StoreWriteBenchmarks>(args: args);
        BenchmarkRunner.Run<StoreReadBenchmarks>(args: args);
    }
}