using Json.More;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteTests
{
    private static readonly string _testsFile = Directories.TestData.CombineFile("jsonpath-compliance-test-suite", "cts.json").FullName;
    private static readonly string[] _notSupported = [];

    //  - id: array_index
    //    pathSegment: $[2]
    //    document: ["first", "second", "third", "forth", "fifth"]
    //    consensus: ["third"]
    //    scalar-consensus: "third"
    public static IEnumerable<TheoryDataRow<ComplianceTestCase>> TestCases
    {
        get
        {
            var fileText = File.ReadAllText(_testsFile);
            var suite = JsonSerializer.Deserialize<ComplianceTestSuite>(fileText, SerializerOptions.Default);
            return suite!.Tests.Select(t => new TheoryDataRow<ComplianceTestCase>(t).WithTestDisplayName(t.Name));
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Run(ComplianceTestCase testCase)
    {
        if (_notSupported.Contains(testCase.Name))
            Assert.Skip("This case will not be supported.");

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(testCase);
        Console.WriteLine();

        if (testCase.InvalidSelector)
        {
            bool tryParseResult;
            try
            {
                tryParseResult = JsonPath.TryParse(testCase.Selector, out _);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail("TryParse() threw an exception");
                throw; // this will never run, but the compiler doesn't know that Assert.Fail() will always throw.
            }
            Assert.False(tryParseResult);

            var exception = Assert.Throws<PathParseException>(() => JsonPath.Parse(testCase.Selector));
            Console.WriteLine($"Error: {exception!.Message}");
            return;
        }

        var path = JsonPath.Parse(testCase.Selector);
        Console.WriteLine("Parse:");
        Evaluate(path, testCase);
        Console.WriteLine();

        var success = JsonPath.TryParse(testCase.Selector, out path);
        Assert.True(success);
        Console.WriteLine("TryParse:");
        Evaluate(path!, testCase);
    }

    private static void Evaluate(JsonPath path, ComplianceTestCase testCase)
    {
        var actual = path.Evaluate(testCase.Document);

        var actualValues = actual.Matches.Select(m => m.Value).ToJsonArray();
        var actualLocations = actual.Matches.Select(m => (JsonValue)m.Location!.ToString()).ToJsonArray();
        Console.WriteLine($"  Actual (values): {JsonSerializer.Serialize(actualValues, SerializerOptions.Default)}");
        Console.WriteLine($"  Actual (locations): {JsonSerializer.Serialize(actualLocations, SerializerOptions.Default)}");
        Console.WriteLine($"  Actual (full): {JsonSerializer.Serialize(actual, SerializerOptions.Default)}");
        if (testCase.InvalidSelector)
            Assert.Fail($"{testCase.Selector} is not a valid path.");

        if (testCase.Result is not null)
        {
            Assert.True(testCase.Result.IsEquivalentTo(actualValues), "Unexpected results returned");
            if (testCase.Location is not null)
                Assert.True(testCase.Location.IsEquivalentTo(actualLocations), "Unexpected results returned");
        }
        else
        {
            Assert.True(testCase.Results!.Contains(actualValues, JsonNodeEqualityComparer.Instance), "None of the options matched.");
            if (testCase.Locations is not null)
                Assert.True(testCase.Locations!.Contains(actualLocations, JsonNodeEqualityComparer.Instance), "None of the options matched.");
        }
    }
}
