namespace FclEx;

internal class Program
{
    internal static void Main(string[] args)
    {
        var config = DefaultConfig.Instance.WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"));
        config.StopOnFirstError(true);

        BenchmarkSwitcher.FromTypes([typeof(SizeOfBenchmark<>)]).Run([.. args, "-f", "*"], config);
    }
}