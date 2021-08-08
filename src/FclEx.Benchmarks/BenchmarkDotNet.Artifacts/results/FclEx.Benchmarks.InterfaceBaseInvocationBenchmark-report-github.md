``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1110 (20H2/October2020Update)
AMD Ryzen 7 5800X, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  DefaultJob : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT


```
|              Method |     Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |---------:|----------:|----------:|-------:|------:|------:|----------:|
|                Base | 3.576 μs | 0.0683 μs | 0.0813 μs | 0.1526 |     - |     - |      3 KB |
| BaseByDynamicMethod | 1.635 μs | 0.0199 μs | 0.0177 μs | 0.0973 |     - |     - |      2 KB |
