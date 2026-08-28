namespace Xunit.v3;

[TestClass(DisableParallelization = true)]
[TestMethodOrderer(typeof(PriorityOrderer))]
[DefaultPriority(10)]
public class PriorityOrdererTests
{
    private static readonly List<string> ExecutedTests = [];

    [Fact]
    [Priority(-1)]
    public void Executes_Explicit_Negative_Priority_First()
    {
        ExecutedTests.Add(nameof(Executes_Explicit_Negative_Priority_First));
    }

    [Fact]
    [Priority(0)]
    public void Executes_Explicit_Priority_Before_Default_Priority()
    {
        ExecutedTests.Add(nameof(Executes_Explicit_Priority_Before_Default_Priority));
    }

    [Fact]
    public void Executes_Default_Priority()
    {
        ExecutedTests.Add(nameof(Executes_Default_Priority));
    }

    [Fact]
    [Priority(10)]
    public void Executes_Explicit_Default_Priority()
    {
        ExecutedTests.Add(nameof(Executes_Explicit_Default_Priority));
    }

    [Fact]
    [Priority(11)]
    public void Orders_Test_Cases_By_Priority_And_Name()
    {
        Assert.SkipWhen(ExecutedTests.IsEmpty(), "No test methods were executed.");

        Assert.Equal(
        [
            nameof(Executes_Explicit_Negative_Priority_First),
            nameof(Executes_Explicit_Priority_Before_Default_Priority),
            nameof(Executes_Default_Priority),
            nameof(Executes_Explicit_Default_Priority),
        ],
        ExecutedTests);
    }
}
