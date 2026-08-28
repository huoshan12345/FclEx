using System.Text;
using System.Text.Json;
using FclEx.Utils;

namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class JsonValidatorBenchmark
{
    private string _json = null!;
    private byte[] _utf8Json = null!;

    [Params(JsonInput.SmallValid, JsonInput.LargeValid, JsonInput.EarlyInvalid, JsonInput.LateInvalid)]
    public JsonInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _json = Input switch
        {
            JsonInput.SmallValid => "{\"id\":42,\"name\":\"fcl-ex\",\"active\":true,\"tags\":[\"core\",\"json\"]}",
            JsonInput.LargeValid => CreateLargeJson(),
            JsonInput.EarlyInvalid => "{\"id\":,\"name\":\"fcl-ex\",\"active\":true}",
            JsonInput.LateInvalid => CreateLargeJson()[..^1],
            _ => throw new ArgumentOutOfRangeException(nameof(Input), Input, null)
        };
        // Keep UTF-8 transcoding out of the reader benchmark.
        _utf8Json = Encoding.UTF8.GetBytes(_json);

        var fclExResult = JsonValidator_IsValid();
        var systemTextJsonResult = JsonDocument_Parse();
        var utf8JsonReaderResult = Utf8JsonReader_Validate();
        if (fclExResult != systemTextJsonResult || fclExResult != utf8JsonReaderResult)
            throw new InvalidOperationException($"Validators disagree for {Input}.");
    }

    [Benchmark]
    public bool JsonValidator_IsValid()
    {
        return JsonValidator.IsValid(_json);
    }

    [Benchmark(Baseline = true)]
    public bool JsonDocument_Parse()
    {
        try
        {
            using var document = JsonDocument.Parse(_json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [Benchmark]
    public bool Utf8JsonReader_Validate()
    {
        try
        {
            var reader = new Utf8JsonReader(_utf8Json);
            if (!reader.Read())
                return false;

            while (reader.Read()) { }
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string CreateLargeJson()
    {
        var items = Enumerable.Range(0, 1_000)
            .Select(i => $"{{\"id\":{i},\"name\":\"item-{i}\",\"active\":{(i % 2 == 0 ? "true" : "false")}}}");

        return $"{{\"items\":[{string.Join(',', items)}]}}";
    }

    public enum JsonInput
    {
        SmallValid,
        LargeValid,
        EarlyInvalid,
        LateInvalid
    }
}
