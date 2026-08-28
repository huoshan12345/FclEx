namespace Xunit.v3;

public class PriorityOrderer : ITestMethodOrderer
{
    private static readonly ConcurrentDictionary<string, int> _defaultPriorities = new();

    private static int PriorityForTest(ITestMethod testMethod, int defaultPriority)
    {
        if (testMethod is not XunitTestMethod xunitTestMethod)
            return int.MaxValue;

        var priorityAttribute = xunitTestMethod.Method
            .GetCustomAttributes<PriorityAttribute>()
            .SingleOrDefault();

        return priorityAttribute?.Priority ?? defaultPriority;
    }

    private static int DefaultPriorityForClass(ITestMethod testMethod)
    {
        return _defaultPriorities.GetOrAdd(testMethod.TestClass.UniqueID, _ => DefaultPriorityForClassCore(testMethod));

        static int DefaultPriorityForClassCore(ITestMethod testMethod)
        {
            if (testMethod is not XunitTestMethod xunitTestMethod)
                return int.MaxValue;

            var defaultAttribute = xunitTestMethod.Method.DeclaringType?
                .GetCustomAttributes<DefaultPriorityAttribute>()
                .SingleOrDefault();

            return defaultAttribute?.Priority ?? int.MaxValue;
        }
    }

    private static readonly IComparer<ITestMethod> _testMethodComparer = ComparerBuilder
        .For<ITestMethod>()
        .Member()
        .OrderBy(x => x.MethodName, StringComparer.OrdinalIgnoreCase)
        .Build();

    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods) where TTestMethod : ITestMethod
    {
        var groupedTestCases = MultiValueDictionary<int, ITestMethod>.Create(() => new Heap<ITestMethod>(0, _testMethodComparer));

        foreach (var testCase in testMethods.NotNull())
        {
            var defaultPriority = DefaultPriorityForClass(testCase);
            var priority = PriorityForTest(testCase, defaultPriority);
            groupedTestCases.Add(priority, testCase);
        }

        return groupedTestCases
            .OrderBy(m => m.Key)
            .SelectMany(m => m.Value)
            .Cast<TTestMethod?>()
            .ToArray();
    }
}