namespace FclEx;

internal class Program
{
    private static void Main(string[] args)
    {
        //TestRandom();
        //return;

        var config = DefaultConfig.Instance.WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"));
        config.StopOnFirstError(true);

        // BenchmarkSwitcher.FromTypes([typeof(SizeOfBenchmark<>)]).Run([.. args, "-f", "*"], config);
        // new IntToByteArrayBenchmark().ExplicitLayoutStruct();
        BenchmarkRunner.Run<JsonValidatorBenchmark>(config);
    }
}