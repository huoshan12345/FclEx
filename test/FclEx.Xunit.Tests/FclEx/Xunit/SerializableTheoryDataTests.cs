#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable

using Xunit;

namespace FclEx.Xunit;

public class SerializableTheoryDataTests
{
    public record Person(string Name, int Age)
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public static TheoryData<Person> TestCases = [new("Tom", 10), new("Jim", 20), new("Tim", 30)];

    [LocalOnlyTheory]
    [MemberData(nameof(TestCases))]
    public async Task NonSerializable_Test(Person person) // cases won't be executed parallelly using ParallelTestFramework
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    public static readonly SerializableTheoryData<Person> SerializableTestCases = [new("Tom", 10), new("Jim", 20), new("Tim", 30)];

    [LocalOnlyTheory]
    [MemberData(nameof(SerializableTestCases))]
    public async Task Serializable_Test(XunitSerializable<Person> person) // cases will be executed parallelly using ParallelTestFramework
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
    }
}
