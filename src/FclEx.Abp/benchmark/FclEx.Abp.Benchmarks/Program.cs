using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using FclEx.Abp.Benchmarks.Data;

internal class Program
{
#pragma warning disable CS1998
    private static async Task Main(string[] args)
#pragma warning restore CS1998
    {
        var config = DefaultConfig.Instance.WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"));
        BenchmarkRunner.Run<EntityInsertBenchmark>(config);
    }
}