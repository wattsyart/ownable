# Benchmarks

## StoreWriteBenchmarks

| Method | Count |       Mean |    Error |  StdDev |  Allocated |
|------- |------ |-----------:|---------:|--------:|-----------:|
| Append |    50 |   100.1 ms |  1.31 ms | 1.16 ms |  158.33 KB |
| Append |   100 |   207.4 ms |  3.54 ms | 3.32 ms |  315.75 KB |
| Append |  1000 | 2,204.6 ms | 11.86 ms | 9.90 ms | 3149.39 KB |

## StoreReadBenchmarks

|    Method | Count |        Mean |     Error |   StdDev |      Median | Ratio | RatioSD |  Allocated | Alloc Ratio |
|---------- |------ |------------:|----------:|---------:|------------:|------:|--------:|-----------:|------------:|
|   GetById |    50 |    94.88 us |  4.133 us | 11.79 us |    90.95 us |  1.00 |    0.00 |  101.25 KB |        1.00 |
| FindByKey |    50 |   146.12 us |  5.285 us | 15.08 us |   139.65 us |  1.57 |    0.26 |  155.16 KB |        1.53 |
|           |       |             |           |          |             |       |         |            |             |
|   GetById |   100 |   177.21 us |  5.102 us | 14.31 us |   179.10 us |  1.00 |    0.00 |  202.03 KB |        1.00 |
| FindByKey |   100 |   271.32 us |  5.594 us | 15.59 us |   268.90 us |  1.54 |    0.15 |  309.84 KB |        1.53 |
|           |       |             |           |          |             |       |         |            |             |
|   GetById |  1000 | 1,665.22 us | 32.026 us | 31.45 us | 1,664.05 us |  1.00 |    0.00 | 2016.09 KB |        1.00 |
| FindByKey |  1000 | 2,662.50 us | 52.645 us | 56.33 us | 2,661.25 us |  1.60 |    0.05 | 3094.22 KB |        1.53 |