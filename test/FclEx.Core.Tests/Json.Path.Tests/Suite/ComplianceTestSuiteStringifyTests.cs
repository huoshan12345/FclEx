using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteStringifyTests
{
    private static readonly string _testsFile = Directories.TestData.CombineFile("jsonpath-compliance-test-suite", "cts.json").FullName;

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
            var suite = JsonSerializer.Deserialize<ComplianceTestSuite>(fileText, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
            });
            return suite!.Tests.Where(x => !x.InvalidSelector)
                .Select(t => new TheoryDataRow<ComplianceTestCase>(t).WithTestDisplayName(t.Name));
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Stringify(ComplianceTestCase testCase)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(testCase);
        Console.WriteLine();

        StringifyTests.AssertStringify(testCase.Selector, testCase.Document);
    }
}
